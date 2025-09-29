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
            List<Actor> globalActors = new List<Actor>();
            foreach (LDtkLevel.Layer layer in level.LayerInstances)
            {
                if (layer.AutoLayerTiles.Count > 0)
                    _actors.Add(createInfos.InstantiateTilemapLayer(layer));
                if (layer.EntityInstances.Count > 0)
                    LoadEntities(createInfos, layer.EntityInstances, globalActors);
            }
            return globalActors;
        }

        public void Update(float deltaTime)
        {
            foreach (Actor actor in _actors)
                actor.OnUpdate(deltaTime);
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (Actor actor in _actors)
                actor.OnLateUpdate(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (Actor actor in _actors)
                actor.OnFixedUpdate(fixedDeltaTime);
        }

        public void Draw()
        {
            for (int i = _actors.Count - 1; i > -1; --i)
                _actors[i].OnDraw();
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