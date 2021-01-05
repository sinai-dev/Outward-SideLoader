using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Model
{
    public interface IPrefabTemplate<T>
    {
        bool IsCreatingNewID { get; }
        bool DoesTargetExist { get; }

        void CreatePrefab();

        T TargetID { get; }
        T NewID { get; }
    }
}
