# RecipeVault

This repository contains the backend API and frontend UI for the RecipeVault project.

## Security & Secrets

Sensitive keys (API keys, database URIs, etc.) should **never** be committed to Git. Configuration files like `appsettings.*.json` are ignored by default.

### Removing existing secrets

If a secret is ever added and pushed, rotate the credential immediately and then purge the value from history using a tool such as `git filter-repo` or the `git filter-branch` command shown earlier. Don't forget to force-push and inform collaborators.

### Preventing future leaks

We include a basic Git pre-commit hook in `.githooks/pre-commit` that scans staged files for common secret patterns and aborts the commit if anything suspicious is found. To activate it for your local clone:

```sh
# set the hooks directory (one-time per clone)
git config core.hooksPath .githooks
chmod +x .githooks/pre-commit
```

You can expand the regex patterns or replace the script with [git-secrets](https://github.com/awslabs/git-secrets) or a similar tool.

Also consider using environment variables, Azure Key Vault, user secrets, or other vaults rather than storing values in JSON files.

## Workflow Tips

- Keep `appsettings.Development.json` and any `.env` files in `.gitignore`.
- For CI, supply secrets via the platform's secure variables mechanism.
- Regularly scan history for accidental exposures using `git grep`, `truffleHog`, or GitHub's secret scanning service.
