using System.Numerics;
using PlatformerGame.Engine.Level;
using PlatformerGame.Engine.Resources;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame
{
    public class Fruit : SpriteActor
    {
        public Fruit(Sprite sprite, int id, Vector2 position, bool active = true)
            : base(sprite, id, position, active)
        {
        }

        public class CreateInfo : CreateInfo<Fruit>
        {
            public override string EntityIdentifier => "Collectable";

            public override Actor Create(ResourceManager resources, LDtkDefinition.Entity def, Vector2 position)
            {
                Sprite sprite = resources.Get<Sprite>(def.TilesetId);
                return new Fruit(sprite, def.UId, position);
            }
        }
    }
}