using System.Numerics;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public class Scene
    {
        private LDtkLevelInfo _info;

        public Scene(LDtkLevelInfo info)
        {
            Actors = new List<Actor>();
            _info = info;
        }

        public string IId => _info.IId;
        public string Identifier => _info.Identifier;

        public int Width => _info.Width;
        public int Height => _info.Height;
        public int WorldX => _info.WorldX;
        public int WorldY => _info.WorldY;

        public Vector2 WorldOffset => new Vector2(_info.WorldX, _info.WorldY);
        public Vector2 Size => new Vector2(_info.Width, _info.Height);
        public List<Actor> Actors { get; }
        public List<LDtkLevelInfo.Neighbour> Neighbours => _info.Neighbours;

        public void AddActors(List<Actor> actors)
        {
            Actors.AddRange(actors);
        }

        public List<Actor> Load(CreateActorRegistry createInfos, LDtkLevel level)
        {
            // Required due to possible inserting of entities before loading scene data
            int tilemapLayerInsertPos = Actors.Count;
            var tilemaps = new List<TilemapLayer>();

            var globalActors = new List<Actor>();
            foreach (LDtkLevel.Layer layer in level.LayerInstances)
            {
                if (layer.AutoLayerTiles.Count > 0)
                    tilemaps.Add(createInfos.InstantiateTilemapLayer(layer, this));
                if (layer.EntityInstances.Count > 0)
                    LoadEntities(createInfos, layer.EntityInstances, globalActors);
            }

            foreach (TilemapLayer layer in tilemaps)
                Actors.Insert(tilemapLayerInsertPos, layer);

            return globalActors;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < Actors.Count(); i++)
            {
                Actor actor = Actors[i];
                if (actor.Destroy)
                {
                    actor.OnDestroy();
                    actor.OnDispose();
                    Actors.RemoveAt(i);
                }
                actor.OnUpdate(deltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            for (int i = 0; i < Actors.Count(); i++)
            {
                Actor actor = Actors[i];
                if (actor.Destroy)
                {
                    actor.OnDestroy();
                    actor.OnDispose();
                    Actors.RemoveAt(i);
                }
                actor.OnLateUpdate(deltaTime);
            }
        }

        public void Draw()
        {
            foreach (Actor actor in Actors)
                actor.OnDraw();
        }

        private void LoadEntities(CreateActorRegistry createInfos, List<LDtkLevel.Entity> entities, List<Actor> globalActors)
        {
            foreach (LDtkLevel.Entity entity in entities)
            {
                bool isGlobal;
                Actor actor = createInfos.Instantiate(entity, this, out isGlobal);
                if (isGlobal)
                    globalActors.Add(actor);
                else
                    Actors.Add(actor);
            }
        }
    }
}