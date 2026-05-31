# RecipeVault

> **Full-stack recipe management app with AI-powered suggestions**

[![Live Demo](https://img.shields.io/badge/Live%20Demo-recipe--vault--flax.vercel.app-58a6ff?style=flat&logo=vercel)](https://recipe-vault-flax.vercel.app)
[![API Docs](https://img.shields.io/badge/API%20Docs-Swagger-85ea2d?style=flat&logo=swagger)](https://recipevault-api-rtvd.onrender.com/swagger)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

RecipeVault lets users save, organize, and discover recipes — with Gemini AI generating smart suggestions based on ingredients on hand. Full-stack: .NET 8 REST API, React + Vite frontend, PostgreSQL, deployed on Render and Vercel.

---

## ✨ Features

- **Recipe CRUD** — create, read, update, delete recipes with full validation
- **AI Suggestions** — Gemini API generates recipe ideas from a list of ingredients
- **Search & Filter** — search by name, filter by category, paginated results
- **JWT Authentication** — secure token-based auth with refresh flow
- **Google SSO** — one-click sign-in via Google OAuth
- **Role-based Access Control** — Admin and User roles with scoped permissions
- **Dockerized** — single-command local setup via Docker Compose

---

## 🏗️ Architecture

```
┌─────────────────────┐      ┌──────────────────────┐
│   React + Vite      │ ───► │   ASP.NET Core API   │
│   Tailwind CSS      │      │   .NET 8             │
│   Vercel            │      │   Render (Docker)    │
└─────────────────────┘      └──────────┬───────────┘
                                         │
                              ┌──────────┴───────────┐
                              │   PostgreSQL (prod)   │
                              │   SQLite (local dev)  │
                              └──────────────────────┘
                                         │
                              ┌──────────┴───────────┐
                              │   Gemini API         │
                              │   (AI suggestions)   │
                              └──────────────────────┘
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | C# · .NET 8 · ASP.NET Core · EF Core |
| Frontend | React · Vite · Tailwind CSS |
| Database | PostgreSQL (prod) · SQLite (dev) |
| Auth | JWT · Google OAuth 2.0 · RBAC |
| AI | Gemini API |
| DevOps | Docker · GitHub Actions · Render · Vercel |

---

## 🚀 Quick Start

**Prerequisites:** Docker, or .NET 8 SDK + Node 18+

```bash
# Clone
git clone https://github.com/Hamza2497/RecipeVault.git
cd RecipeVault

# Set environment variables — fill in GEMINI_API_KEY, GOOGLE_CLIENT_ID, connection string
cp appsettings.json appsettings.Development.json

# Run with Docker
docker build -t recipevault . && docker run -p 5000:80 recipevault

# Or run manually
dotnet run
cd recipevault-ui && npm install && npm run dev
```

API runs at `http://localhost:5000` · Frontend at `http://localhost:5173`

---

## 📁 Project Structure

```
RecipeVault/
├── Controllers/              # REST endpoints
├── Services/                 # Business logic + Gemini integration
├── Models/                   # EF Core entities
├── Migrations/               # Database migrations
├── Repositories/             # Data access layer
├── DTOs/                     # Request / response models
├── recipevault-ui/           # React + Vite frontend
├── Dockerfile
└── Program.cs
```

---

## 🌐 Deployment

| Service | Platform | URL |
|---------|----------|-----|
| Frontend | Vercel | [recipe-vault-flax.vercel.app](https://recipe-vault-flax.vercel.app) |
| Backend API | Render (Docker) | [recipevault-api-rtvd.onrender.com](https://recipevault-api-rtvd.onrender.com/swagger) |
| Database | Render PostgreSQL | — |

---

## 👤 Author

**Hamza Assaf** — [hamza2497.github.io](https://hamza2497.github.io) · [LinkedIn](https://linkedin.com/in/hamzah-assaf)
