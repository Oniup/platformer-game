using System.Numerics;
using PlatformerGame.Engine.Serialization;

namespace PlatformerGame.Engine.Level
{
    public class Scene
    {
        private List<Actor> _actors;
        private LDtkLevelInfo _info;

        public Scene(LDtkLevelInfo info)
        {
            _actors = new List<Actor>();
            _info = info;
        }

        public string IId
        {
            get { return _info.IId; }
        }

        public string Identifier
        {
            get { return _info.Identifier; }
        }

        public int Width
        {
            get { return _info.Width; }
        }

        public int Height
        {
            get { return _info.Height; }
        }

        public int WorldX
        {
            get { return _info.WorldX; }
        }

        public int WorldY
        {
            get { return _info.WorldY; }
        }

        public Vector2 WorldOffset
        {
            get { return new Vector2(_info.WorldX, _info.WorldY); }
        }

        public Vector2 Size
        {
            get { return new Vector2(_info.Width, _info.Height); }
        }

        public List<Actor> Actors
        {
            get { return _actors; }
        }

        public List<LDtkLevelInfo.Neighbour> Neighbours
        {
            get { return _info.Neighbours; }
        }

        public void AddActors(List<Actor> actors)
        {
            _actors.AddRange(actors);
        }

        public List<Actor> Load(CreateActorRegistry createInfos, LDtkLevel level)
        {
            // Required due to possible inserting of entities before loading scene data
            int tilemapLayerInsertPos = _actors.Count;
            List<TilemapLayer> tilemaps = new List<TilemapLayer>();

            List<Actor> globalActors = new List<Actor>();
            foreach (LDtkLevel.Layer layer in level.LayerInstances)
            {
                if (layer.AutoLayerTiles.Count > 0)
                    tilemaps.Add(createInfos.InstantiateTilemapLayer(layer, this));
                if (layer.EntityInstances.Count > 0)
                    LoadEntities(createInfos, layer.EntityInstances, globalActors);
            }

            foreach (TilemapLayer layer in tilemaps)
                _actors.Insert(tilemapLayerInsertPos, layer);

            return globalActors;
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < _actors.Count(); i++)
            {
                Actor actor = _actors[i];
                if (actor.Destroy)
                {
                    actor.OnDestroy();
                    _actors.RemoveAt(i);
                }
                actor.OnUpdate(deltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            for (int i = 0; i < _actors.Count(); i++)
            {
                Actor actor = _actors[i];
                if (actor.Destroy)
                {
                    actor.OnDestroy();
                    _actors.RemoveAt(i);
                }
                actor.OnLateUpdate(deltaTime);
            }
        }

        public void Draw()
        {
            foreach (Actor actor in _actors)
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
                    _actors.Add(actor);
            }
        }
    }
}