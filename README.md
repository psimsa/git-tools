# Git Repo Tools 
[![CodeFactor](https://www.codefactor.io/repository/github/psimsa/git-tools/badge)](https://www.codefactor.io/repository/github/psimsa/git-tools)

A small extension to git. Currently contains 2 commands:

`git repo nuke` - Removes all branches except master/main (or optionally current) from local repository, resets changes, runs `git prune` and pulls latest remote. 
Motivation: I work on several projects at the same time and after a while my repos tend to get cluttered with branches that were once feature/bugfix branches, then
got merged by someone, are not on the upstream anymore but I still have them locally. So this command basically refreshes the repo to a like-new state.

`git repo bootstrap` - WIP, I want to be able to easily parameterize creation of new repos using templates. Future work though.