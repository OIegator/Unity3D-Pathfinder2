using System.Collections;
using Priority_Queue;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// По-хорошему это октодерево должно быть, но неохота.
/// Класс, владеющий полной информацией о сцене - какие области где расположены, 
/// как связаны между собой, и прочая информация.
/// Должен по координатам точки определять номер области.
/// </summary>

namespace BaseAI
{
    /// <summary>
    /// Базовый класс для реализации региона - квадратной или круглой области
    /// </summary>
    public interface IBaseRegion
    {
        /// <summary>
        /// Индекс региона - соответствует индексу элемента в списке регионов
        /// </summary>
        int index { get; set; }

        /// <summary>
        /// Список соседних регионов (в которые можно перейти из этого)
        /// </summary>
        IList<IBaseRegion> Neighbors { get; set; }
        
        /// <summary>
        /// Принадлежит ли точка региону (с учётом времени)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        bool Contains(PathNode node);
        
        /// <summary>
        /// Является ли регион динамическим
        /// </summary>
        bool Dynamic { get; }

        void TransformPoint(PathNode parent, PathNode node);

        /// <summary>
        /// Квадрат расстояния до ближайшей точки региона (без учёта времени)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        void TransformToLocal(PathNode node);

        float SqrDistanceTo(PathNode node);

        /// <summary>
        /// Добавление времени транзита через регион
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        void AddTransferTime(IBaseRegion source, IBaseRegion dest);

        /// <summary>
        /// Время перехода через область насквозь, от одного до другого 
        /// </summary>
        /// <param name="source">Регион, с границы которого стартуем</param>
        /// <param name="transitStart">Глобальное время начала перехода</param>
        /// <param name="dest">Регион назначения - ближайшая точка</param>
        /// <returns>Глобальное время появления в целевом регионе</returns>
        float TransferTime(IBaseRegion source, float transitStart, IBaseRegion dest);
        
        /// <summary>
        /// Центральная точка региона - используется для марштуризации
        /// </summary>
        /// <returns></returns>
        Vector3 GetCenter();

        Collider Collider { get; }

        List<PathNode> FindPath(PathNode start, PathNode target, MovementProperties movementProperties, PathFinder context);
    }

    /// <summary>
    /// Сферический регион на основе SphereCollider
    /// </summary>
    public class SphereRegion : IBaseRegion
    {
        /// <summary>
        /// Тело региона - коллайдер
        /// </summary>
        public SphereCollider body;

        public Collider Collider => body;

        public Platform1Movement PlatformMovement;
        /// <summary>
        /// Расстояние транзита через регион
        /// </summary>
        private Dictionary<System.Tuple<int, int>, string> transits;

        /// <summary>
        /// Индекс региона в списке регионов
        /// </summary>
        public int index { get; set; } = -1;

        bool IBaseRegion.Dynamic { get; } = false;
        void IBaseRegion.TransformPoint(PathNode parent, PathNode node) { return; }

        void IBaseRegion.TransformToLocal(PathNode node) { }

        public IList<IBaseRegion> Neighbors { get; set; } = new List<IBaseRegion>();


        public SphereRegion(SphereCollider sample)
        {
            body = sample;
        }

        /// <summary>
        /// Квадрат расстояния до региона (минимально расстояние до границ коллайдера в квадрате)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public float SqrDistanceTo(PathNode node) { return body.bounds.SqrDistance(node.Position); }
        /// <summary>
        /// Проверка принадлежности точки региону
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Contains(PathNode node) { return body.bounds.Contains(node.Position); }



        /// <summary>
        /// Время перехода через область насквозь, от одного до другого 
        /// </summary>
        /// <param name="source">Регион, с границы которого стартуем</param>
        /// <param name="transitStart">Глобальное время начала перехода</param>
        /// <param name="dest">Регион назначения - ближайшая точка</param>
        /// <returns>Глобальное время появления в целевом регионе</returns>
        public float TransferTime(IBaseRegion source, float transitStart, IBaseRegion dest) {
            throw new System.NotImplementedException();
        }

        public Vector3 GetCenter() 
        {
            //  Вроде бы должно работать
            return body.bounds.center;
        }

        void IBaseRegion.AddTransferTime(IBaseRegion source, IBaseRegion dest)
        {
            throw new System.NotImplementedException();
        }

        enum State
        {
            NotStarted,
            JumpedOnPlatform,
            JumpOnShore,
        }

        private State state = State.NotStarted;

        public List<PathNode> FindPath(PathNode start, PathNode target, MovementProperties movementProperties, PathFinder context)
        {
            if (state == State.NotStarted)
            {

                var predicted = new PathNode(PlatformMovement.PredictLocation(1.0f), start.Direction, true);
                var dist = context.Distance(predicted, start, movementProperties);
                if (dist < 8.0)
                {
                    state = State.JumpedOnPlatform;
                    predicted.JumpNode = true;
                    return new List<PathNode> { predicted };
                }
            }
            else if (state == State.JumpedOnPlatform)
            {
                var dist = context.Distance(start, target, movementProperties);
                if (dist < 10.0)
                {
                    Debug.Log("Can jump from platform");
                    state = State.JumpedOnPlatform;
                    target.JumpNode = true;
                    return new List<PathNode> { target };
                }
            }

            var result = new List<PathNode>();

            return result;
        }
    }
    
    /// <summary>
    /// Сферический регион на основе BoxCollider
    /// </summary>
    public class BoxRegion : IBaseRegion
    {
        /// <summary>
        /// Тело коллайдера для представления региона
        /// </summary>
        public BoxCollider body;

        public Collider Collider => body;

        /// <summary>
        /// Индекс региона в списке регионов
        /// </summary>
        public int index { get; set; } = -1;
        
        bool IBaseRegion.Dynamic { get; } = false;
        void IBaseRegion.TransformPoint(PathNode parent, PathNode node) { return; }
        void IBaseRegion.TransformToLocal(PathNode node) { }
        public IList<IBaseRegion> Neighbors { get; set; } = new List<IBaseRegion>();
        
        /// <summary>
        /// Создание региона с кубическим коллайдером в качестве основы
        /// </summary>
        /// <param name="RegionIndex"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public BoxRegion(BoxCollider sample)
        {
            body = sample;
        }
        
        /// <summary>
        /// Квадрат расстояния до региона (минимально расстояние до границ коллайдера в квадрате)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public float SqrDistanceTo(PathNode node) { return body.bounds.SqrDistance(node.Position); }
        
        /// <summary>
        /// Проверка принадлежности точки региону
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Contains(PathNode node) { return body.bounds.Contains(node.Position); }
        
        /// <summary>
        /// Время перехода через область насквозь, от одного до другого 
        /// </summary>
        /// <param name="source">Регион, с границы которого стартуем</param>
        /// <param name="transitStart">Глобальное время начала перехода</param>
        /// <param name="dest">Регион назначения - ближайшая точка</param>
        /// <returns>Глобальное время появления в целевом регионе</returns>
        public float TransferTime(IBaseRegion source, float transitStart, IBaseRegion dest)
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetCenter()
        {
            //  Вроде бы должно работать
            return body.bounds.center;
        }

        void IBaseRegion.AddTransferTime(IBaseRegion source, IBaseRegion dest)
        {
            throw new System.NotImplementedException();
        }


        public List<PathNode> FindPath(PathNode start, PathNode target, MovementProperties movementProperties, PathFinder context)
        {
            var result = new List<PathNode>();

            var nodes = new SimplePriorityQueue<PathNode>();
            nodes.Enqueue(start, Vector3.Distance(start.Position, target.Position));

            PathNode res = null;

            var i = 0;
            var minDist = float.MaxValue;
            PathNode minDistNode = null;
            while (nodes.Count > 0 && i < 10000)
            {
                i++;
                var current = nodes.Dequeue();
                if (Vector3.Distance(current.Position, target.Position) < movementProperties.deltaTime * movementProperties.maxSpeed / movementProperties.closeEnslowment)
                {
                    res = current;
                    break;
                }


                var backupSpeed = movementProperties.maxSpeed;
                if (Vector3.Distance(current.Position, target.Position) < movementProperties.targetClose)
                {
                    movementProperties.maxSpeed = backupSpeed / movementProperties.closeEnslowment;
                }

                var neighbours = context.GetNeighbours(current, movementProperties);
                foreach (var neighbor in neighbours)
                {
                    var newDist = Vector3.Distance(neighbor.Position, target.Position);
                    nodes.Enqueue(neighbor, newDist);
                    if (newDist < minDist)
                    {
                        minDist = newDist;
                        minDistNode = neighbor;
                        res = minDistNode;
                    }

                }

                movementProperties.maxSpeed = backupSpeed;
            }

            if (res == null)
            {
                result.Add(minDistNode);
            }
            else
            {
                Debug.Log($"Count of steps = {i}");
                while (res != null)
                {
                    result.Add(res);
                    res = res.Parent;
                }

                result.Reverse();
            }

            Debug.Log("Маршрут обновлён");
            Debug.Log("Финальная точка маршрута : " + result[result.Count - 1].Position.ToString() + "; target : " + target.Position.ToString());
            return result;

        }
    }

    public class Cartographer
    {
        //  Список регионов
        public List<IBaseRegion> regions = new List<IBaseRegion>();

        //  Поверхность (Terrain) сцены
        public Terrain SceneTerrain;

        // Start is called before the first frame update
        public Cartographer(GameObject collidersCollection)
        {
            //  Получить Terrain. Пробуем просто найти Terrain на сцене
            try
            {
                SceneTerrain = (Terrain)Object.FindObjectOfType(typeof(Terrain));
            }
            catch (System.Exception e)
            {
                Debug.Log("Can't find Terrain!!!" + e.Message);
            }

            //  Создаём региончики
            //  Они уже созданы в редакторе, как коллекция коллайдеров - повешена на объект игровой сцены CollidersMaster внутри объекта Surface
            //  Их просто нужно вытащить списком, и запихнуть в список регионов.
            //  Но есть проблема - не перепутать индексы регионов! Нам нужно вручную настроить списки смежности - какой регион с
            //  каким граничит. Это можно автоматизировать, как-никак у нас коллайдеры с наложением размещены, но вообще это
            //  не сработает для динамических регионов (коллайдеры которых перемещаются) - они автоматически не установят связи.
            //  Поэтому открываем картинку RegionsMap.png в корне проекта, и ручками дорисовываем регионы, и связи между ними.

            var colliders = collidersCollection.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                IBaseRegion region;

                switch (collider)
                {
                    case BoxCollider boxCollider:
                        region = new BoxRegion(boxCollider);
                        break;
                    case SphereCollider sphereCollider:
                        region = new SphereRegion(sphereCollider)
                        {
                            PlatformMovement = sphereCollider.gameObject.GetComponent<Platform1Movement>()
                        };
                        break;
                    default:
                        throw new System.Exception("Only Box and Sphere colliders are allowed.");
                }

                regions.Add(region);
                regions[regions.Count - 1].index = regions.Count - 1;
            }


            for (var i = 0; i < regions.Count; i++)
                for (var j = i + 1; j < regions.Count; j++)
                    if (regions[i].Collider.bounds.Intersects(regions[j].Collider.bounds))
                    {
                        regions[i].Neighbors.Add(regions[j]);
                        regions[j].Neighbors.Add(regions[i]);
                    }

            for (var i = 0; i < regions.Count; i++)
            {
                Debug.Log(
                    $"Region : {i} ({regions[i].GetType()}, {regions[i].GetCenter()}) -> {string.Join(", ", regions[i].Neighbors.Select(it => it.index))}");
            }

        }

        /// <summary>
        /// Регион, которому принадлежит точка. Сделать абы как
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Индекс региона, -1 если не принадлежит (не проходима)</returns>
        public IBaseRegion GetRegion(PathNode node)
        {
            for (var i = 0; i < regions.Count; ++i)
                //  Метод полиморфный и для всяких платформ должен быть корректно в них реализован
                if (regions[i].Contains(node))
                    return regions[i];
            Debug.Log("Not found region for " + node.Position.ToString());
            return null;
        }
    }
}