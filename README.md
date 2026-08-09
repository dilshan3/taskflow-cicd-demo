# TaskFlow

[![CI](https://github.com/dilshan3/taskflow-cicd-demo/actions/workflows/ci.yml/badge.svg)](https://github.com/dilshan3/taskflow-cicd-demo/actions/workflows/ci.yml)

A small **Task Management API** built with ASP.NET Core (.NET 8). It exists to teach
**CI/CD**: every push is automatically built, tested, and deployed to Azure by a
GitHub Actions pipeline.

Tasks live in memory (no database), so the whole thing runs with a single command.
Open the site and you land straight on the interactive Swagger docs.

## What it does

A `TaskItem` has a title, description, priority (Low/Medium/High), status
(Todo/InProgress/Done), a due date, and a created timestamp. The business rules
live in `TaskService`:

- **Creating a task** rejects empty titles, titles over 100 characters, and due
  dates in the past.
- **Changing status** only allows `Todo -> InProgress -> Done`. You cannot skip
  from Todo straight to Done, and you cannot move backwards out of Done.
- **Overdue** means the due date has passed and the task is not yet Done.

### Endpoints

| Method | Route                     | Purpose                          |
| ------ | ------------------------- | -------------------------------- |
| GET    | `/api/tasks`              | List all tasks                   |
| GET    | `/api/tasks/overdue`      | List overdue tasks               |
| GET    | `/api/tasks/{id}`         | Get one task                     |
| POST   | `/api/tasks`              | Create a task                    |
| PUT    | `/api/tasks/{id}/status`  | Move a task to a new status      |
| DELETE | `/api/tasks/{id}`         | Delete a task                    |

## Run it locally

```bash
dotnet run --project src/TaskFlow
```

Then open the URL it prints (for example `http://localhost:5080`) to use Swagger.
Six sample tasks are loaded on startup, so there is data to play with right away.

## Run the tests

```bash
dotnet test
```

The tests in `tests/TaskFlow.Tests` cover the `TaskService` rules: title and
due-date validation, every valid and invalid status transition, and overdue
detection.

## How this pipeline works

The pipeline is defined in [`.github/workflows/ci.yml`](.github/workflows/ci.yml)
and runs on GitHub's servers every time you push or open a pull request against
`main`. It has two jobs.

**1. `build-and-test`** — runs on every push *and* every pull request:

1. Checks out your code.
2. Installs the .NET 8 SDK.
3. `dotnet restore` — downloads the NuGet packages.
4. `dotnet build` — compiles the app and the tests.
5. `dotnet test` — runs the unit tests. **If any test fails, the pipeline stops
   here** and the red X on your commit or PR tells everyone the change is not safe
   to merge. This is the "CI" (Continuous Integration) half: every change is
   proven to build and pass tests before it goes anywhere.

**2. `deploy`** — the "CD" (Continuous Deployment) half:

- `needs: build-and-test` means it only starts **after** tests pass. Broken code
  never reaches Azure.
- The `if:` condition means it runs **only for pushes to `main`**, not for pull
  requests. Opening a PR tests your code; merging it ships your code.
- It publishes a Release build and deploys to the Azure App Service
  `cicddemo` using `azure/webapps-deploy`.

### The one thing you have to set up

The deploy job authenticates to Azure with a **secret** called
`AZURE_WEBAPP_PUBLISH_PROFILE` (Settings -> Secrets and variables -> Actions).
Until that secret is added, `build-and-test` passes but `deploy` fails — that is
expected. Secrets are how a pipeline holds credentials without ever putting them
in the code.

### The mental model

```
push / PR  ->  build  ->  test  ->  (only on main, only if tests pass)  ->  deploy to Azure
```

Watch it happen on the [Actions tab](https://github.com/dilshan3/taskflow-cicd-demo/actions).
