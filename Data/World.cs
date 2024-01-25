﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project1.Data
{
    internal class World
    {
        private static FieldInfo worldField = typeof(SystemComponent).GetField("_world", BindingFlags.Instance | BindingFlags.NonPublic);

        public string WorldName { get; private set; }
        private SparceIndexedList<Entity> _entities;
        private Dictionary<Type, List<EntityComponent>> _components;
        private Dictionary<Type, SystemComponent> _systems;

        public World(string name) 
        {
            WorldName = name;
            _entities = new SparceIndexedList<Entity>();
            _components = new Dictionary<Type, List<EntityComponent>>();
            _systems = new Dictionary<Type, SystemComponent>();
        }

        public World AddSystem(SystemComponent system)
        {
            worldField.SetValue(system, this);
            _systems[system.GetType()] = system;
            return this;
        }

        public Entity GetEntity(int Id)
        {
            return _entities[Id];
        }

        public unsafe T[] GetEntityComponents<T> () where T : EntityComponent
        {
            if (_components.ContainsKey(typeof(T)))
            {
                // TODO : this might be too slow
                return Unsafe.As<T[]>(_components[typeof(T)].ToArray());
            }
            return null;
        }

        public T GetSystem<T>() where T : SystemComponent
        {
            if (_systems.ContainsKey(typeof(T)))
            {
                return (T)_systems[typeof(T)];
            }
            return null;
        }

        public T RegisterEntityComponent<T>(T component) where T : EntityComponent
        {
            Type t = typeof(T);
            
            //TODO : figure a way to remove this silly hack
            if (t.BaseType != typeof(EntityComponent))
                t = t.BaseType;
            
            if (!_components.ContainsKey(t))
                _components[t] = new List<EntityComponent>();
            _components[t].Add(component);
            return component;
        }

        public void UnRegisterEntityComponent(EntityComponent component)
        {
            Type t = component.GetType();
            if (_components.ContainsKey(t))
                _components[t].Remove(component);
        }

        public Entity CreateEntity()
        {
            Entity entity = new Entity(this);
            entity.Id = _entities.Add(entity);
            return entity;
        }

        public void Initalize()
        {
            foreach (var x in _systems.Values)
                x.Initalize();
        }

        public void Update(GameTime deltaTime)
        {
            foreach (var x in _systems.Values)
                x.Update(deltaTime);
            //foreach(var x in _components.Keys)
            //    foreach(var y in _components[x])
            //        y.Update(deltaTime);
        }

        public void Draw(GameTime deltaTime)
        {
            foreach (var x in _systems.Values)
                x.Draw(deltaTime);
            //foreach (var x in _components.Keys)
            //    foreach (var y in _components[x])
            //        y.Draw(deltaTime);
        }

        public void Close()
        {
            foreach (var x in _systems.Values)
                x.Close();
            foreach (var x in _entities)
                x.Close();
            _entities.Clear();
        }


    }
}
