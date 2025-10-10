using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Level.Collision;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class PlatformTilemapLayer : TilemapLayer
    {
        List<RegisteredInside> _inside;

        public PlatformTilemapLayer(SpriteAtlas atlas, CollisionLayer layer, CollisionLayer mask, List<LDtkLevel.Tile> tiles, Vector2 position)
            : base(atlas, layer, mask, tiles, position)
        {
            _cellCollider.Height = 5;
            _inside = new List<RegisteredInside>();
        }

        public override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            for (int i = 0; i < _inside.Count; i++)
            {
                if (!_inside[i].KeepRegistered)
                {
                    _inside.RemoveAt(i);
                    continue;
                }
                _inside[i].KeepRegistered = false;
            }
        }

        public bool IsRegistered(CharacterActor actor)
        {
            foreach (RegisteredInside registered in _inside)
            {
                if (actor == registered.Actor)
                    return true;
            }
            return false;
        }

        protected override bool ApplyDisplacement(CollisionShapeActor actor, ShapeCollider collider, Vector2 thisDisplacement, ref Vector2 displacement)
        {
            CharacterActor? charActor = actor as CharacterActor;
            if (charActor != null)
            {
                for (int i = 0; i < _inside.Count; i++)
                {
                    if (_inside[i].Actor == charActor)
                    {
                        _inside[i].KeepRegistered = true;
                        return false;
                    }
                }
                // Not already inside
                if (charActor.Velocity.Y <= 0.0f)
                {
                    _inside.Add(new RegisteredInside()
                    {
                        KeepRegistered = true,
                        Actor = charActor,
                    });
                    return false;
                }
            }

            displacement += thisDisplacement;
            return true;
        }

        private class RegisteredInside
        {
            public bool KeepRegistered;
            public CharacterActor Actor = null!;
        }

        public new class CreateInfo : CreateInfo<PlatformTilemapLayer>
        {
            public override string LayerIdentifier => "Platform";

            public override TilemapLayer Instantiate(ResourceManager resources, LDtkDefinition.Tileset tileset, LDtkDefinition.Layer def, List<LDtkLevel.Tile> tiles, Vector2 worldPosition)
            {
                SpriteAtlas atlas = resources.Get<SpriteAtlas>(tileset.UId);
                return new PlatformTilemapLayer(atlas, CollisionLayer.Ground | CollisionLayer.Platform, CollisionLayer.None, tiles, worldPosition);
            }
        }
    }
}