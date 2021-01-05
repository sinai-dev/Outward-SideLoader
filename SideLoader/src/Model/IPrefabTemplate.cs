using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Model
{
    public interface IPrefabTemplate<T, U>
    {
        bool IsCreatingNewID { get; }
        bool DoesTargetExist { get; }

        void CreatePrefab();

        U TargetID { get; }
        U AppliedID { get; }
    }
}
