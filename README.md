# Golf Course NPC Ball Collector

Unity 3D demo showcasing **predictive AI decision-making** with dynamic strategy switching and intelligent target commitment.

## 🎮 Features

### Core Gameplay
- Autonomous NPC that collects golf balls with 3 priority levels (10/20/30 points)
- Health system with regeneration at golf cart base
- NavMesh-based pathfinding with obstacle avoidance
- Real-time score tracking and strategy display UI

**Demo Video:** [Watch Gameplay](https://drive.google.com/file/d/1dZt2zutqZPxMmsbBm99USX4APtOVeQJ_/view?usp=sharing)

**Windows Build:** `Golf-Course-NPC-Ball-Collector\Build\Golf Course NPC Ball Collector.exe`

---

## 🧠 Decision-Making System ⭐

### **Predictive AI with Hybrid Commitment Strategy**

Unlike reactive AI systems that only respond to current conditions, this NPC uses **forward prediction** to forecast outcomes before committing to decisions.

#### 🔮 **Forward Prediction Model**

Before selecting any target, the NPC calculates:
```python
PredictedFinalHealth = CurrentHealth - (TravelTime × HealthDrainRate)

Where:
  TravelTime = (DistanceToBall + DistanceBackToCart) / AverageSpeed
  HealthDrainRate = 1 HP per second
  AverageSpeed = 3.5 units/second
```

**Example Calculation:**
```
Current Health: 80 HP
Target: Level 3 ball at 40 units
Return distance: 40 units back to cart
Total travel: 80 units

Prediction:
  Time = 80 / 3.5 = 22.8 seconds
  Health loss = 22.8 × 1 = 22.8 HP
  Final health = 80 - 22.8 = 57.2 HP ✅ Safe to proceed
```

---

#### 🎯 **Hybrid Target Commitment**

The NPC demonstrates **intelligent commitment** balanced with **adaptive safety**:

**COMMITMENT MODE (Default):**
- Once a target is selected, the NPC commits to completing it
- Strategy changes mid-journey do NOT trigger re-evaluation
- Demonstrates planning and follow-through

**EMERGENCY OVERRIDE (Critical Situations):**
- If predicted final health drops below **15% critical threshold**
- NPC abandons current target and re-routes to safer option
- Prioritizes survival over commitment

**Decision Flow:**
```
Strategy Change Detected
    ↓
Calculate Predicted Health at Target Completion
    ↓
IF PredictedHealth < 15% (CRITICAL)
    → EMERGENCY: Re-evaluate and switch target
ELSE
    → COMMITTED: Continue to current target
```

**Real-World Example:**
```
Scenario 1: Non-Critical Strategy Change
Health 75% → Greedy selects distant Level 3 ball (50 units)
Walking... Health drops to 65% → Strategy switches to Balanced
Prediction: Trip will end at 45% health
Decision: ✅ COMMIT (45% > 15% threshold) - Complete original plan

Scenario 2: Critical Emergency
Health 40% → Balanced selects Level 3 ball (60 units)  
Walking... Health drops to 25% → Strategy switches to Survival
Prediction: Trip will end at 5% health
Decision: ⚠️ EMERGENCY (5% < 15% threshold) - Abort and re-route!
    → New target: Nearest Level 1 ball (safe distance)
```

---

### **Three Dynamic Strategies:**

#### 💰 **1. Greedy Strategy** (Health > 70%)
**Philosophy:** Maximize score while healthy

**Decision Logic:**
- Always targets highest point-value ball
- Distance is secondary consideration (tie-breaker only)
- No health-cost evaluation
- Assumes sufficient health buffer for any trip

**Typical Behavior:**
```
Available: Level 1 (5 units), Level 2 (20 units), Level 3 (50 units)
Selection: Level 3 (30 pts) - ignores 10x distance penalty
```

---

#### ⚖️ **2. Balanced Strategy** (30-70% Health)
**Philosophy:** Optimize risk/reward ratio with predictive safety

**Decision Logic:**
```
UtilityScore = (PointValue × HealthMultiplier) / (EstimatedHealthCost + 1)

Where:
  HealthMultiplier = CurrentHealth / 100
  EstimatedHealthCost = TravelTime × HealthDrainRate
  
Safety Rules:
  - Reject if EstimatedHealthCost >= CurrentHealth (impossible trip)
  - Reject if FinalHealth < 20% (safety margin)
```

**Typical Behavior:**
```
Current Health: 50 HP

Ball A: Level 3 (30 pts), 60 units away
  → Cost: 34 HP, Final: 16 HP ❌ REJECT (below 20% margin)

Ball B: Level 2 (20 pts), 30 units away  
  → Cost: 17 HP, Final: 33 HP ✅ ACCEPT
  → Utility: (20 × 0.5) / 17 = 0.59

Ball C: Level 1 (10 pts), 15 units away
  → Cost: 8.5 HP, Final: 41.5 HP ✅ ACCEPT
  → Utility: (10 × 0.5) / 8.5 = 0.59

Selection: Ball B (higher point value, same utility)
```

---

#### 🛡️ **3. Safety-First Strategy** (Health < 30%)
**Philosophy:** Survival mode - stay near base

**Decision Logic:**
- **Primary Rule:** Only consider balls within 15 units of golf cart (safe zone)
- **Fallback Rule:** If no safe balls exist, pick absolute closest (emergency)

**Typical Behavior:**
```
Balls in safe zone (< 15 units from cart):
  → Level 1 (8 units from cart) ✅ Selected (closest in safe zone)
  → Level 2 (12 units from cart)

Balls outside safe zone:
  → Level 3 (40 units from cart) ❌ Ignored (too far from safety)

If safe zone empty:
  → Emergency fallback: Pick closest ball regardless (survival attempt)
```

---

## 🏗️ Architecture

### Design Patterns Used

| Pattern | Implementation | Purpose |
|---------|---------------|---------|
| **Strategy** | `ICollectionStrategy` + 3 concrete strategies | Pluggable AI decision-making algorithms |
| **State Machine** | `NPCBrain` FSM (Search → Move → Return) | Clear behavior flow control |
| **Observer** | C# events (`OnHealthChanged`, `OnScoreChanged`, `OnStrategyChanged`) | Decouple UI from game logic |
| **Interface Segregation** | `ICollectable` interface | Future extensibility for new collectible types |

### Architecture Diagram
```
┌─────────────────────────────────────────────────────┐
│              GAME LOGIC LAYER                       │
├─────────────────────────────────────────────────────┤
│  NPCBrain (FSM Controller)                          │
│    ├─ States: Search → Move → Return                │
│    ├─ Strategy Selector (health-based)              │
│    └─ Hybrid Commitment Logic                       │
│         ├─ Predictive health calculation            │
│         └─ Emergency override (< 15% threshold)     │
│                                                      │
│  ICollectionStrategy (Interface)                    │
│    ├─ GreedyStrategy (max points)                   │
│    ├─ BalancedStrategy (utility scoring)            │
│    └─ SafetyFirstStrategy (survival)                │
│                                                      │
│  HealthSystem ──→ OnHealthChanged (event)           │
│  ScoreSystem ──→ OnScoreChanged (event)             │
│  NPCBrain ──→ OnStrategyChanged (event)             │
└─────────────────────────────────────────────────────┘
                        │
                   Scene Reference
                        │
┌─────────────────────────────────────────────────────┐
│                  UI LAYER                           │
├─────────────────────────────────────────────────────┤
│  UIManager (Event Subscriber)                       │
│    ├─ UpdateHealthDisplay() ← OnHealthChanged       │
│    ├─ UpdateScoreDisplay() ← OnScoreChanged         │
│    └─ UpdateStrategyDisplay() ← OnStrategyChanged   │
└─────────────────────────────────────────────────────┘
```
### Key Architectural Benefits

✅ **Loose Coupling:** UI changes don't require game logic modifications
✅ **Testability:** Each strategy can be unit tested independently  
✅ **Extensibility:** New strategies add via interface implementation (Open/Closed Principle)
✅ **Maintainability:** Clear separation of concerns (FSM, Strategy, UI, Systems)

---

## 🛠️ Technical Implementation

### Tech Stack
- **Unity Version:** 2021.3.5f1
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Pathfinding:** Unity NavMesh with runtime path validation
- **Animation:** PrimeTween for lightweight UI transitions
### Project Structure
```
Assets/_Project/
├── Scripts/
│   ├── Core/
│   │   ├── HealthSystem.cs          (Health tracking + events)
│   │   ├── ScoreSystem.cs           (Score tracking + events)
│   │   └── UIManager.cs             (Event-driven UI controller)
│   └── Feature/
│       ├── Behaviors/
│       │   ├── ICollectionStrategy.cs   (Strategy interface)
│       │   ├── GreedyStrategy.cs        (Max points algorithm)
│       │   ├── BalancedStrategy.cs      (Predictive utility scoring)
│       │   ├── SafetyFirstStrategy.cs   (Safe zone logic)
│       │   └── NPCBrain.cs              (FSM + Strategy Selector)
│       ├── Entities/
│       │   ├── ICollectable.cs          (Collectable interface)
│       │   └── GolfBall.cs              (Ball implementation)
│       └── UI/
│           └── BillboardHealthBar.cs    (World-space health display)
├── Scenes/
│   └── Main.unity
├── Prefabs/
└── Materials/
```

### Architecture Organization

**Core Layer:**
- Shared systems used across features
- Health, Score, and UI management
- Game-wide services

**Feature Layer:**
- Domain-specific implementations
- `Behaviors/`: AI decision-making strategies
- `Entities/`: Collectible game objects
- `UI/`: Feature-specific UI components

This separation follows **Clean Architecture** principles:
- Core contains framework-level systems
- Features contain business logic
- Clear dependency direction: Features → Core (never reverse)

## 🏗️ Scene Hierarchy Structure

The scene is organized into functional layers to ensure scalability and ease of navigation. This structure follows the "Separation of Concerns" principle, keeping environmental, logical, and dynamic elements in distinct containers.

```
Main (Scene)
├── ⚙️ SceneSetup            # Global environment & configuration
│   ├── Main Camera          # Primary viewpoint
│   ├── Directional Light    # Global illumination
│   ├── URP Volume Profile   # Post-processing & visual effects
│   └── EventSystem          # Input handling for UI
├── 🌿 Envo (Environment)    # Static world geometry
│   ├── Terrain              # Ground mesh
│   ├── Bridge               # Structural assets
│   └── Water                # Environmental shaders
├── 🖥️ UI (User Interface)   # Screen-space elements
│   └── Canvas               # Main UI container
│       ├── GameplayGUI      # HUD (Score, Health, Strategy)
│       ├── StartPanel       # Initial landing state
│       └── SettingsPanel    # Configuration options
├── 🧠 Systems               # Logical controllers (Managers)
│   ├── UIManager            # Bridges game events to UI updates
│   ├── HealthSystem         # Manages NPC vitals & logic
│   ├── ScoreSystem          # Tracks collection progress
│   └── Camera               # Camera follow/utility logic
└── 🤖 Entities              # Dynamic actors & objects
    ├── GolfCart (Target)    # Delivery point & healing station
    ├── NPC                  # Autonomous agent with AI Brain
    └── Balls                # Collectible object container
```
Why This Structure?
Logical Separation: Systems are decoupled from visuals, making it easier to swap environments without breaking the game logic.

Performance: Grouping static objects under Envo simplifies Static Batching and Occlusion Culling management.

Developer Experience: The organized hierarchy allows for instant navigation, crucial for maintaining clean projects during technical assessments.

---

## 🎯 Key Design Decisions

### Why Predictive AI?

**Traditional Reactive AI:**
```
NPC checks health → Picks target → Starts moving → Dies en route ❌
```

**This Implementation (Predictive):**
```
NPC checks health → Simulates trip outcome → Rejects unsafe targets → Survives ✅
```

**Benefits:**
- Prevents "suicidal" decisions (e.g., 30% health pursuing 80-unit distant target)
- Forward-looking intelligence vs. reactive behavior
- Demonstrates planning capability, not just reaction

---

### Why Hybrid Commitment?

**Pure Commitment (Never Re-evaluate):**
- ✅ Predictable, efficient
- ❌ Can die if conditions worsen unexpectedly

**Pure Dynamic (Always Re-evaluate):**
- ✅ Maximally adaptive
- ❌ Appears indecisive, wastes resources backtracking

**Hybrid (This Implementation):**
- ✅ Commits to plans (shows intelligence)
- ✅ Aborts only when survival threatened (emergency response)
- ✅ Best of both worlds: planning + adaptability

**Critical Threshold Rationale:**
```
15% chosen as emergency threshold because:
  - Below 15%: High risk of death before reaching cart
  - Above 15%: Sufficient buffer to complete trip safely
  - Configurable in code for different risk profiles
```

---

### Health Cost Calculation

**Formula Breakdown:**
```
HealthCost = Distance × DrainRate / Speed

Where:
  Distance = DistanceToBall + DistanceToCart (round trip)
  DrainRate = 1 HP/second (constant depletion)
  Speed = 3.5 units/second (NavMeshAgent speed)
```

**Why Round-Trip Calculation?**
- NPC must return to cart to score points
- One-way calculation would be incomplete (NPC dies on return)
- Ensures NPC always plans for complete objective loop

---

## 🚀 How to Run

### From Unity Editor
1. Open project in Unity 2021.3.5f1
2. Navigate to `Assets/_Project/Scenes/Main.unity`
3. Press **Play** button
4. Observe:
    - Top UI: Current strategy (Greedy/Balanced/Survival)
    - Console logs: Decision-making process
    - NPC behavior: Target selection, emergency overrides

### From Build
1. Navigate to: `Golf-Course-NPC-Ball-Collector\Build\`
2. Run `Golf Course NPC Ball Collector.exe` (Windows)
3. Watch autonomous NPC behavior

---

## 📊 Testing Variations

Modify these Inspector values (NPCBrain component) to test different AI personalities:

### Aggressive Risk-Taker
```
Greedy Threshold: 60%  (enters greedy mode earlier)
Safety Threshold: 20%  (delays survival mode)
Health Restore: 5      (less recovery, more risk)
```
**Expected Behavior:** Pursues high-value targets aggressively, takes more risks

### Conservative Survivor
```
Greedy Threshold: 80%  (rarely enters greedy mode)
Safety Threshold: 40%  (early survival activation)
Health Restore: 20     (more recovery, cautious)
```
**Expected Behavior:** Prioritizes safety, slower score accumulation

### Default Balanced
```
Greedy Threshold: 70%
Safety Threshold: 30%
Health Restore: 10
```
**Expected Behavior:** Optimal risk/reward balance

---

## 🧪 Observable AI Behaviors

### Console Log Examples

**Predictive Calculation:**
```
🎯 Balanced: Ball_Level3_5 | HP: 80 → 57 | Utility: 1.45
```
Translation: Selected Level 3 ball, predicts health will be 57 after trip

**Strategy Switch (Non-Critical):**
```
🔄 Strategy Changed: Balanced (Health: 65%)
✅ Strategy changed but committed to current target (Predicted HP: 45)
```
Translation: Strategy changed but current target still safe - commitment maintained

**Emergency Override:**
```
🔄 Strategy Changed: Survival (Health: 25%)
⚠️ EMERGENCY! Predicted health (8) too low. Abandoning target.
🔄 Emergency re-route: Ball_Level3_12 → Ball_Level1_3
```
Translation: Critical situation detected - aborting dangerous target

**Safety Rejection:**
```
⚠️ Balanced: No safe targets!
```
Translation: All available balls would result in death - waiting for health regen

---

## 📝 Implementation Notes

- **Health Drain:** 1 HP/second continuous (pauses when health = 0)
- **Cart Regeneration:** +10 HP per successful delivery (configurable)
- **Ball Values:** Level 1 = 10pts, Level 2 = 20pts, Level 3 = 30pts
- **NavMesh Validation:** Pre-checks path validity to prevent stuck states
- **Safety Margin:** Balanced strategy maintains 20% minimum health buffer
- **Emergency Threshold:** 15% triggers critical re-evaluation

---

## 🏆 Key Innovations

| Feature | Traditional AI | This Implementation |
|---------|---------------|---------------------|
| Decision Timing | ❌ Reactive (current state) | ✅ Predictive (future state) |
| Strategy Switching | ❌ Immediate re-evaluation | ✅ Hybrid commitment with emergency override |
| Health Consideration | ❌ Distance-based only | ✅ Time-based health forecasting |
| Safety Guarantees | ❌ None (trial and error) | ✅ 20% safety margin + 15% emergency threshold |
| Target Commitment | ❌ None or absolute | ✅ Intelligent balance (commit unless critical) |

---

## 👨‍💻 Developer Notes

**Design Philosophy:**
This project demonstrates **production-quality game AI** principles:
- Predictive modeling over reactive responses
- Configurable risk profiles (Inspector-tweakable thresholds)
- Separation of concerns (Strategy Pattern, Event System, FSM)
- Defensive programming (null checks, path validation, safety margins)

**For Interviewers:** 
The decision-making system showcases:
1. **Algorithm Design:** Utility scoring with multi-factor evaluation
2. **Software Architecture:** SOLID principles, design patterns
3. **Game AI Concepts:** FSM, behavior planning, risk assessment
4. **Unity Best Practices:** Event-driven UI, NavMesh integration, clean project structure

---

**Built as technical demonstration** - Clean architecture, predictive AI, and Unity best practices.
