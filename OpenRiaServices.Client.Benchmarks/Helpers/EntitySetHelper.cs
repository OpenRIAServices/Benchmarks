using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenRiaServices.Client;

namespace ClientBenchmarks.Helpers
{
    class EntitySetHelper
    {
        public static EntitySet<T> CreateEntitySet<T>() where T : Entity
        {
            return CreateEntitySet<T>(EntitySetOperations.All);
        }

        public static EntitySet<T> CreateEntitySet<T>(EntitySetOperations operations) where T : Entity
        {
            DynamicEntityContainer container = new DynamicEntityContainer();
            return container.AddEntitySet<T>(operations);
        }

        /// <summary>
        /// An dynamic EntityContainer class that allows external configuration of
        /// EntitySets for testing purposes.
        /// </summary>
        public class DynamicEntityContainer : EntityContainer
        {
            public EntitySet<T> AddEntitySet<T>(EntitySetOperations operations) where T : Entity
            {
                base.CreateEntitySet<T>(operations);
                return GetEntitySet<T>();
            }
        }
    }
}
