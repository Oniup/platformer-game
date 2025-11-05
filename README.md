# Platformer Game

Written in C# and uses Raylib as its backend via NuGet

## Assets Used

* [Main Assets](https://pixelfrog-assets.itch.io/pixel-adventure-1)
* [Start](https://soulofkiran.itch.io/pixel-art-animated-star)
* [Fonts](https://not-jam.itch.io/not-jam-font-pack)
* [Prototype Tileset](https://captainlaptop.itch.io/white-prototyping-tileset)

## Design patterns used

> Where is a list of OOP design patterns that are used throughout the program. 
> Some examples are used where these patterns are used within the application.

* Façade    - Application, AnimationController
* Composite - World/Scene with their actors, Canvas, AnimationSet, CollisionShapes
* Observer  - EventDispatcher
* Flyweight - ResourceManager
* Factory   - CreateActorRegistry
* Adapter   - Sprite, SpriteAtlas, Window
* State     - Enemy