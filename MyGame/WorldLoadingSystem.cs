using Microsoft.Xna.Framework;
using Project1.Engine;
using Project1.Engine.Components;
using Project1.Engine.Systems.Physics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project1.MyGame
{
    internal class WorldLoadingSystem : SystemComponent
    {

        public Vector3 PlayerLocation { get; private set; }

        private World _world;
        public WorldLoadingSystem(World world)
        {
            _world = world;
        }

        public void LoadWorld(string path)
        {
            string content = File.ReadAllText(path);
            Console.WriteLine($"Loading world {Path.GetFileName(path)}");
            string[] lines = content.Split("\n");
            Console.WriteLine($" | Loading {lines.Length} empties");
            foreach(var x in lines)
            {
                if (x.Trim().Length != 0)
                    LoadEmpty(x);
            }
        }

        private void LoadEmpty(string line)
        {
            string[] args = line.Split(':');
            string name = args[0];
            Vector3 pos = new Vector3(float.Parse(args[1]), float.Parse(args[3]), float.Parse(args[2]));
            Vector3 scale = new Vector3(float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[4]));
            Vector3 rotation = new Vector3(-float.Parse(args[7]), float.Parse(args[9]), -float.Parse(args[8]));

            Matrix localMatrix = Matrix.CreateScale(scale) *
                            Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                            Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                            Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z));
            Matrix worldMatrix = localMatrix * Matrix.CreateTranslation(pos);

            switch (name.ToLower())
            {
                case "box":
                    var phyx = new PrimitivePhysicsComponent(RigidBody.Box, RigidBodyFlags.Static);
                    _world.CreateEntity()
                        .AddComponent(new PositionComponent(worldMatrix))
                        .AddComponent(phyx);
                    phyx.Restitution = 0.1f;
                    phyx.StaticFriction = .9f;
                    phyx.DynamicFriction = .2f;
                    break;
                case "sphere":
                    _world.CreateEntity()
                        .AddComponent(new PositionComponent(worldMatrix))
                        .AddComponent(new PrimitivePhysicsComponent(RigidBody.Sphere, RigidBodyFlags.Static, scale.Length()));
                    break;
                case "player":
                    PlayerLocation = pos;
                    break;
            }
        }

    }
}
