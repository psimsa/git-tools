# Git Repo Tools

[![CodeFactor](https://www.codefactor.io/repository/github/<secret_hidden>/git-tools/badge)](https://www.codefactor.io/repository/github/<secret_hidden>/git-tools)

A small extension to git that provides useful utilities for repository management. Currently contains 3 commands to help streamline your git workflow.

## Commands

### `git repo nuke`
Completely cleans up your local repository by:
- Removing all branches except master/main (or optionally the current branch)
- Resetting any uncommitted changes
- Running `git prune` to remove stale references
- Pulling the latest changes from remote

**Motivation:** When working on multiple projects simultaneously, local repositories tend to accumulate feature and bugfix branches that have been merged upstream but still exist locally. This command refreshes your repository to a clean, like-new state.

**Usage:**
```bash
git repo nuke [options]
```

**Options:**
- `--quiet, -q`: Do not ask for confirmation
- `--no-switch-branch, -n`: Do not switch to main/master branch
- `--use-branch, -b <branch>`: Use specific branch instead of master/main

### `git repo tidy-branch`
Creates a clean, squashed commit from your current branch and rebases it onto another branch (master/main by default).

**Usage:**
```bash
git repo tidy-branch [options]
```

**Options:**
- `--quiet, -q`: Do not ask for confirmation
- `--target-branch, -t <branch>`: Branch to rebase onto (default: master/main)

### `git repo bootstrap`
Initialize a new repository with customizable parameters using templates.

**Usage:**
```bash
git repo bootstrap [options]
```

**Options:**
- `--template, -t <template>`: Template to use (hidden option)
- `--default-branch, -b <branch>`: Default branch to use (default: main)
- `--user-email, -e <email>`: User email to use

## Installation

This tool is built as a .NET global tool. To install:

```bash
dotnet tool install -g GitRepo
```

## Development

This project is built with .NET and uses the System.CommandLine library for CLI parsing. The source code is organized as follows:

- [`src/GitRepo/Program.cs`](src/GitRepo/Program.cs) - Main entry point and command setup
- [`src/GitRepo/Commands/`](src/GitRepo/Commands/) - Individual command implementations
- [`src/GitRepo/Common/`](src/GitRepo/Common/) - Shared utilities and helpers

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests to improve the tool.
