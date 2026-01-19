# Animal Hunting Game in VR 🏹🦌

> Protect your camp from dangerous animals using bow and arrow in an immersive VR experience!

## 🎮 Overview

Animal Hunting Game is a VR archery game that combines the classic shooting mechanics with tower defense elements. Players must defend their camp from waves of attacking animals using a bow and arrow, creating an adrenaline-pumping experience inspired by games like Plants vs Zombies.

### Key Features
- **Realistic Bow Mechanics**: Fully interactable and pullable bow with haptic feedback
- **Dynamic Animal AI**: Randomly generated animals with intelligent pathfinding
- **Camp Defense**: Protect your base from destruction at all costs
- **Reward System**: Points, combos, and unlockable special arrows
- **Immersive VR**: Spatial audio, environmental effects, and comfortable VR controls

## 👥 Team - GPTeam

- **Ken** - Lead Developer
- **Ziyu** - Developer
- **Subeom** - Developer

**Meeting Schedule**: Twice a week for backlog analysis, task assignment, and work merging.

## 🎯 Target Audience

Players who:
- Love adventures and challenges
- Enjoy archery mechanics
- Want to WIN and feel the thrill

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

## 🛠️ Technical Requirements

### Supported VR Headsets
- Meta Quest 3s/3
- More 

### Development Tools
- Unity 2021.3+ / 6000.3.3f1
- XR Interaction Toolkit
- Version Control: Git

## 🚀 Running the Game
1. Build latest version in "Build" folder
2. Connect your VR headset and install
3. Enjoy hunting!

**Epic Overview:**
- BOW & ARROW - Core shooting mechanics
- ANIMAL - AI, spawning, and interactions
- CAMP - Defense system and feedback
- REWARD - Scoring and progression
- UI & COMFERT - Polish and VR comfort
- OPTIMIZATION - Performance and testing

## 📝 Git Commit Conventions

We follow the Conventional Commits specification for clear and consistent commit history.

### Commit Format
```
<type>: <subject>

<body>

<footer>
```

### Types
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

### Rules
1. **Use imperative mood**: "add" not "added" or "adds"
2. **Capitalize subject line**: Start with uppercase letter
3. **No period at end**: Subject should not end with `.`
4. **Limit subject to 50 characters**: Keep it concise
5. **Separate subject from body**: Use blank line
6. **Wrap body at 72 characters**: For better readability
7. **Explain what and why**: Not how (code shows how)

### Examples

#### Good Commits ✅
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

#### Bad Commits ❌
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

## 📄 License

Currently nah🤪

## ❓ FAQ

Collecting..

---

**Game is Coming Soon!** 🏹🦌

*Making adventures happen anywhere, anytime in VR*
