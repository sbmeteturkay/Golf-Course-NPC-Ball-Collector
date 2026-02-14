# Golf Course NPC Ball Collector

Unity 3D demo showcasing AI decision-making for NPC ball collection with dynamic strategy switching.

## 🎮 Features

### Core Gameplay
- Autonomous NPC that collects golf balls with 3 priority levels (Low/Medium/High points)
- Health system with regeneration at golf cart base
- NavMesh-based pathfinding with obstacle avoidance
- Score tracking and UI display

### Decision-Making System ⭐

The NPC uses a **Strategy Pattern** to dynamically switch collection strategies based on health:

#### 1. **Greedy Strategy** (Health > 70%)
- Prioritizes highest-value balls
- Ignores distance, maximizes points/collection
- Active when NPC is healthy and can afford risks

#### 2. **Balanced Strategy** (30-70% Health)
- **Utility Score Formula:**
```
  Score = (PointValue × HealthMultiplier) / (EstimatedHealthCost + 1)
  
  Where:
    HealthMultiplier = CurrentHealth / 100
    EstimatedHealthCost = TotalDistance × 0.5
    TotalDistance = DistanceToBall + DistanceToCart
```
- Evaluates risk/reward for each target
- Rejects trips that would deplete health
- Default strategy for moderate health levels

#### 3. **Safety-First Strategy** (Health < 30%)
- Only targets balls within 15 units of golf cart
- Prioritizes survival over points
- Falls back to closest ball if no safe options

### Architecture Highlights
- **Event-driven UI** (loose coupling between game logic and display)
- **Strategy Pattern** for extensible AI behaviors
- **Interface-based design** (`ICollectable`, `ICollectionStrategy`)
- **State machine** for NPC behavior management

## 🛠️ Technical Details

- **Unity Version:** 2021.3.5f1
- **Render Pipeline:** URP (Universal Render Pipeline)
- **Pathfinding:** Unity NavMesh
- **Architecture:** Clean architecture with separation of concerns

### Project Structure
```
Assets/_Project/
├── Scripts/
│   ├── Core/
│   │   ├── HealthSystem.cs
│   │   ├── ScoreSystem.cs
│   │   └── NPCBrain.cs
│   ├── Behaviors/
│   │   ├── ICollectionStrategy.cs
│   │   ├── GreedyStrategy.cs
│   │   ├── BalancedStrategy.cs
│   │   └── SafetyFirstStrategy.cs
│   ├── Entities/
│   │   ├── GolfBall.cs
│   │   └── ICollectable.cs
│   └── Managers/
│       └── UIManager.cs
├── Scenes/
├── Prefabs/
└── Materials/
```

## 🎯 Key Design Decisions

### Why Strategy Pattern?
- **Extensibility:** New strategies can be added without modifying existing code
- **Testability:** Each strategy can be tested independently
- **Readability:** Clear separation of decision-making logic
- **Showcase:** Demonstrates SOLID principles and design patterns

### Health Cost Calculation
- Rate: 0.5 health per unit distance
- Includes both outbound (to ball) and return (to cart) distance
- NPC refuses impossible trips (insufficient health)

### Dynamic Strategy Switching
Strategies switch automatically based on health thresholds:
- Greedy threshold: 70% (configurable in Inspector)
- Safety threshold: 30% (configurable in Inspector)
- Allows testing different risk profiles

## 🚀 How to Run

### From Unity
1. Open project in Unity 2021.3.5f1
2. Open `Scenes/MainScene`
3. Press Play

### From Build
1. Extract the build archive
2. Run `GolfBallCollector.exe` (Windows) or `.app` (macOS)
3. Observe NPC behavior and UI strategy display

## 📊 Testing Variations

Modify Inspector values to test different behaviors:

**Aggressive Configuration:**
- Greedy Threshold: 60%
- Safety Threshold: 20%
- Health Restore: 5

**Conservative Configuration:**
- Greedy Threshold: 80%
- Safety Threshold: 40%
- Health Restore: 20

## 📝 Notes

- Health drains at 1 point/second while active
- Returning to golf cart restores 10 health (configurable)
- Ball levels (1-3) determine point values (10-30 points)
- NavMesh validates paths before movement to prevent stuck states

---

**Built for demonstration purposes** - showcasing clean architecture, AI decision-making, and Unity best practices.