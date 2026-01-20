# Animal Hunting Game in VR 🏹🦌

> Protect your camp from dangerous animals using bow and arrow in an immersive VR experience!

---

## 📑 Table of Contents

### For Players
- [Overview](#-overview)
- [Target Audience](#-target-audience)
- [Technical Requirements](#️-technical-requirements)
- [Running the Game](#-running-the-game)
- [FAQ](#-faq)
- [License](#-license)

### For Developers
- [Team](#-team---gpteam)
- [Development Roadmap](#️-development-roadmap)
- [Git Workflow](#-git-workflow)
  - [Commit Conventions](#-git-commit-conventions)
  - [Scene Workflow Rules](#-scene-workflow-rules)
  - [Daily Workflow](#-daily-workflow)
  - [Common Mistakes](#-common-mistakes)

---

# For Players

## 🎮 Overview

Animal Hunting Game is a VR archery game that combines the classic shooting mechanics with tower defense elements. Players must defend their camp from waves of attacking animals using a bow and arrow, creating an adrenaline-pumping experience inspired by games like Plants vs Zombies.

### Key Features
- **Realistic Bow Mechanics**: Fully interactable and pullable bow with haptic feedback
- **Dynamic Animal AI**: Randomly generated animals with intelligent pathfinding
- **Camp Defense**: Protect your base from destruction at all costs
- **Reward System**: Points, combos, and unlockable special arrows
- **Immersive VR**: Spatial audio, environmental effects, and comfortable VR controls

## 🎯 Target Audience

Players who:
- Love adventures and challenges
- Enjoy archery mechanics
- Want to WIN and feel the thrill

## 🛠️ Technical Requirements

### Supported VR Headsets
- Meta Quest 3s/3
- More headsets to be tested..

### Development Tools
- Unity 2021.3+ / 6000.3.3f1
- XR Interaction Toolkit
- Version Control: Git

## 🚀 Running the Game
1. Build latest version in "Build" folder
2. Connect your VR headset and install
3. Enjoy hunting!

## 📄 License

Currently nah🤪

## ❓ FAQ

Collecting..

---

# For Developers

## 👥 Team - GPTeam

- **Ken** - Lead Developer
- **Ziyu** - Developer
- **Subeom** - Developer

**Meeting Schedule**: Twice a week for backlog analysis, task assignment, and work merging.

## 🗺️ Development Roadmap

### Phase 1: Game Demo (Feasibility Check)
**Week 2 (Sat-Sun)**
- Build basic shooting mechanics
- Implement bow interaction and arrow physics

### Phase 2: Full Gameplay Prototype
**Week 3 (Mon-Sun)**
- Base protection system
- Animal movement and AI
- Hit detection and feedback

### Phase 3: Iterate & Improve
**Week 4 (Mon-Thu)**
- Reward system implementation
- Polish and optimization
- Playtesting and balancing

**Epic Overview:**
- BOW & ARROW - Core shooting mechanics
- ANIMAL - AI, spawning, and interactions
- CAMP - Defense system and feedback
- REWARD - Scoring and progression
- UI & COMFERT - Polish and VR comfort
- OPTIMIZATION - Performance and testing

---

## 🔧 Git Workflow

### 📝 Git Commit Conventions

We follow the Conventional Commits specification for clear and consistent commit history.

#### Commit Format
```
<type>: <subject>

<body>

<footer>
```

#### Types
- `feat`: New feature (e.g., `feat: add bow grab mechanic`)
- `fix`: Bug fix (e.g., `fix: resolve arrow collision issues`)
- `docs`: Documentation changes (e.g., `docs: update README with setup instructions`)
- `style`: Code formatting, no logic change (e.g., `style: format arrow physics script`)
- `refactor`: Code restructuring (e.g., `refactor: optimize animal spawning system`)
- `perf`: Performance improvements (e.g., `perf: implement arrow pooling`)
- `test`: Adding or updating tests (e.g., `test: add unit tests for damage calculation`)
- `chore`: Maintenance tasks (e.g., `chore: update Unity version`)
- `build`: Build system changes (e.g., `build: update project settings`)
- `ci`: CI/CD changes (e.g., `ci: add automated build pipeline`)

#### Rules
1. **Use imperative mood**: "add" not "added" or "adds"
2. **Capitalize subject line**: Start with uppercase letter
3. **No period at end**: Subject should not end with `.`
4. **Limit subject to 50 characters**: Keep it concise
5. **Separate subject from body**: Use blank line
6. **Wrap body at 72 characters**: For better readability
7. **Explain what and why**: Not how (code shows how)

#### Examples

**Good Commits ✅**
```bash
feat: implement bow string pull mechanic

Add hand tracking for bowstring interaction with haptic feedback.
This allows players to feel tension when pulling the string.

Closes #15

---

fix: prevent arrows from passing through animals

Improved collision detection by adding continuous collision detection
to arrow rigidbody and refining animal hitboxes.

Fixes #42

---

perf: optimize animal spawning with object pooling

Reduces instantiation overhead by reusing animal objects.
Frame rate improved by ~15 FPS during wave peaks.

---

docs: add Git commit conventions to README
```

**Bad Commits ❌**
```bash
# Too vague
fix: bug fix

# Past tense instead of imperative
feat: added bow mechanic

# Not capitalized
feat: implement shooting

# Has period at end
fix: collision issue.

# Too long subject (should be split)
feat: implement bow grab mechanic and add haptic feedback and create visual indicators
```

---

### 🎬 Scene Workflow Rules

**READ THIS FIRST! 🚨**

#### The Basic Idea

Everyone works in their own scene. We only change MainScene together during team meetings.

#### File Structure
```
Scenes/
  MainScene.unity              # DON'T TOUCH (except team meetings)
  Your_Workspace.unity         # YOUR scene - edit freely
  Teammate_Workspace.unity     # NOT yours - don't touch
```

#### 3 Simple Rules

**✅ Rule 1: Only Edit Your Own Scene**
- Work in `YourName_Workspace.unity`
- Don't open or change other people's scenes
- Don't change `MainScene.unity` when working alone

**⚠️ Rule 2: Check Before You Commit**

Before every commit, run:
```bash
git status
```

**If you see `MainScene.unity` changed:**
- Did we just have a team meeting? → OK to commit
- Working alone? → You made a mistake! Do this:
```bash
git checkout -- Assets/Scenes/MainScene.unity
```
This removes your MainScene changes.

**📦 Rule 3: Use Prefabs**

When your feature is ready:
1. Make it a Prefab in `Assets/Prefabs/`
2. During team meeting, drag Prefab into MainScene together
3. Test it
4. Then commit

#### Why These Rules?

**The Problem:**
- Unity scene files break easily when 2+ people edit them
- Git can't fix Unity scene conflicts
- You'll waste hours fixing merge errors

**The Solution:**
- Everyone has their own scene = no conflicts!
- Prefabs are easier to merge
- We only touch MainScene together = safe

---

### 💼 Daily Workflow

#### When You Start Working
```bash
git pull origin main
```
Open `YourName_Workspace.unity` and work there.

#### Before You Commit
```bash
git status
```

**Good ✅:**
```
modified: Assets/Scenes/YourName_Workspace.unity
modified: Assets/Prefabs/MonsterSpawner.prefab
modified: Assets/Scripts/MonsterSpawner.cs
```

**Bad ❌:**
```
modified: Assets/Scenes/MainScene.unity  ← PROBLEM!
```

If you see MainScene changed and we didn't have a meeting:
```bash
git checkout -- Assets/Scenes/MainScene.unity
```

#### Example: Adding Monster Spawner

**Step 1 - Work in your scene:**
- Open `YourName_Workspace.unity`
- Add MonsterSpawner
- Test it works

**Step 2 - Make Prefab:**
- Drag MonsterSpawner to `Assets/Prefabs/`
- Name it `MonsterSpawner.prefab`

**Step 3 - Commit your work:**
```bash
git status  # Check what changed
git add .
git commit -m "add monster spawner"
git push
```

**Step 4 - Team meeting:**
- Show your Prefab to the team
- Together, drag it into MainScene
- Test it
- One person commits MainScene

---

### 🔴 Common Mistakes

#### Mistake 1: "I accidentally changed MainScene"
**Fix:**
```bash
git checkout -- Assets/Scenes/MainScene.unity
```

#### Mistake 2: "Git says merge conflict in MainScene"
**Fix:**
1. Tell the team in Discord/Slack
2. Don't force push!
3. We'll fix it together

#### Mistake 3: "I want to add something to MainScene"
**Fix:**
Wait for team meeting! Or ask everyone if they're available now.

---

### 📋 Quick Reference Card

```
✅ DO:
- Work in your own scene
- Make Prefabs
- Check git status before commit

❌ DON'T:
- Touch MainScene alone
- Edit other people's scenes
- Force push when there's a conflict

🆘 HELP:
- Changed MainScene by mistake? → git checkout -- Assets/Scenes/MainScene.unity
- Merge conflict? → Ask team for help
- Not sure? → Ask before committing!
```

### Questions?

Ask in Discord/Slack before:
- Changing MainScene
- If you see unexpected files in `git status`
- If you get any errors

**Remember: It's better to ask than to break the project! 💪**

---
