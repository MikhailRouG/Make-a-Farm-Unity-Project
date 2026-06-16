# 🌾 Make-a-Farm — Multiplayer Farming Game

A real-time multiplayer farming game prototype built with **Unity 6** and **Mirror Networking**.  
Players can farm, trade, and drive vehicles together in a shared world, all synchronized through a server-authoritative architecture.

[![Unity](https://img.shields.io/badge/Unity-6%20(URP)-black?logo=unity)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-purple?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Mirror](https://img.shields.io/badge/Mirror-Networking-blue)](https://mirror-networking.gitbook.io/)
[![Zenject](https://img.shields.io/badge/Zenject-DI-green)](https://github.com/modesttree/Zenject)

---

## Demo

[![Make-a-Farm Demo](https://img.youtube.com/vi/trwps3u5K54/maxresdefault.jpg)](https://www.youtube.com/watch?v=9UDYlf44tX0)

---

## Screenshots

<!-- Replace the paths below with your actual screenshot files -->
<!-- Place images in a Screenshots/ folder at the repository root -->

![Gameplay Screenshot 1](https://github.com/user-attachments/assets/1ce622f8-c394-4c91-adce-013f72eb1785)
![Gameplay Screenshot 2](https://github.com/user-attachments/assets/1919b6c8-896f-4ec1-bca1-e77a169cc2bb)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🌐 **Multiplayer Networking** | Mirror Host/Client — player spawn, sync, and disconnect handling |
| 🌱 **Farming System** | Seed → growth stages → harvest, fully network-synced with ghost placement preview |
| 🎒 **Synced Inventory** | 9-slot inventory via `SyncList<InventorySlot>` with item stacking and UI binding |
| 🏪 **Shop System** | Dynamic shop cards generated via Zenject Factory pattern |
| 🚗 **Vehicle System** | WheelCollider physics, enter/exit, Cinemachine camera switch |
| 🖱️ **Interaction System** | Raycast + `IInteractable` interface — unified control for doors, crops, shop, vehicles |
| 📦 **Item Database** | ScriptableObject-based items injected via Zenject, searchable by ID |
| 🚀 **Rendering Optimization** | GPU Instancing + Static Batching — **Draw Calls reduced 286 → 88 (~70%), FPS 111 → 135+** |
| 🎮 **Player Controller** | Camera-relative movement, sprint, jump, animation — New Input System |

---

## 🏗️ Architecture

Server-authoritative flow using Mirror Networking:

```
Client Input (WASD / E key / LMB)
    │
    ▼
[Command] ──────────────────► Server validation & processing
                                        │
                           ┌────────────┴────────────┐
                           ▼                         ▼
                     SyncVar / SyncList         NetworkServer.Spawn()
                     (state sync)               (object creation)
                           │                         │
                           └────────────┬────────────┘
                                        ▼
                                Broadcast to all clients
                                        │
                                        ▼
                               Client UI update & VFX
```

### Design Patterns Used

| Pattern | Where | Purpose |
|---------|-------|---------|
| `NetworkBehaviour` inheritance | `Plant`, `Inventory`, `Player` | Mirror automatic state sync |
| Command pattern | All client→server actions | Consistent server-authoritative action handling |
| Interface design | `IInteractable`, `IHarvestable` | Decoupled interaction targets |
| Factory (DI) | `ShopCardUi.Factory` | Runtime dynamic UI generation |
| ScriptableObject data | `ItemConfig` inheritance tree | Add items without code changes |
| Dependency Injection | `GameplaySceneInstaller` (Zenject) | Loose coupling, no service locators |

---

## 📁 Project Structure

```
Assets/Scripts/
├── Gameplay/
│   ├── Player/          # Input, movement, interaction, placement, inventory UI (8 scripts)
│   ├── FarmScripts/     # Crop growth, harvest logic, ghost validation (6 scripts)
│   ├── Inventory/       # SyncList inventory, slots, UI (4 scripts)
│   ├── Items/           # ItemConfig abstract base, seeds, food, DB builder (5 scripts)
│   └── Zenject/         # DI config & scene installer (2 scripts)
└── Menu/                # Main menu & loading screen (2 scripts)

Assets/Data/             # ScriptableObject assets (2 seeds, 3 foods, DB)
Assets/Scenes/           # MainMenu.unity / GameScene.unity
Assets/Prefab/           # Player, Plant, UI prefabs
```

---

## 🛠️ Tech Stack

- **Engine:** Unity 6 (URP)
- **Language:** C#
- **Networking:** Mirror Networking (server-authoritative)
- **DI Framework:** Zenject
- **Camera:** Cinemachine
- **Input:** Unity New Input System
- **Physics:** CharacterController, WheelCollider

---

## 🚧 Planned Features

- In-game currency & shop economy system
- Player data save / load (persistence)
- Sound effects & ambient audio
- Watering, fertilizer & crop health system
- Player-to-player trading & chat

---

## 🚀 Getting Started

1. Clone this repository
2. Open the project in **Unity 6** (URP)
3. Install packages via Package Manager: Mirror, Zenject (Extenject), Cinemachine, New Input System
4. Open `Assets/Scenes/MainMenu.unity` and press **Play**
5. Use **Host** to start a server+client, or **Client** to join an existing session

---

## 👤 Author

**Mikhail Pavlov** — Game Engineer  
[GitHub](https://github.com/mikhailroug) · [Portfolio](https://github.com/mikhailroug/Make-a-Farm-Unity-Project)
