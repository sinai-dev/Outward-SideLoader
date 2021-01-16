using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Model
{
    public class DependancySolver<T, U> where T : IPrefabTemplate<U>
    {
        public void ApplyTemplates(List<T> allTemplates)
        {
            if (allTemplates == null || !allTemplates.Any())
                return;

            List<T> sorted;

            // Check if there are any actual dependencies
            var dependencies = allTemplates.Where(it => !it.DoesTargetExist).ToList();

            if (dependencies.Any())
            {
                // Toplogical sort dependencies and full list.
                if (!TopologicalSort(allTemplates, dependencies, out List<T> resolved))
                {
                    // sort returning false means some templates weren't resolved.
                    // these will still exist in the "dependencies" list.
                    foreach (var template in dependencies)
                        SL.LogWarning("A template targeting ID '" + template.TargetID + "' could not be resolved! This may be a circular dependency.");
                }

                sorted = resolved;
            }
            else
                sorted = allTemplates; // we had no dependencies

            foreach (var template in sorted)
            {
                try
                {
                    template.CreatePrefab();
                }
                catch (Exception e)
                {
                    SL.LogWarning("Exception applying template!");
                    SL.LogInnerException(e);
                }
            }
        }

        internal static bool TopologicalSort(List<T> allTemplates, List<T> dependencies, out List<T> output)
        {
            output = new List<T>();

            var sorting = new HashSet<T>(allTemplates.Where(it => it.DoesTargetExist));

            while (sorting.Any())
            {
                var solvedItem = sorting.First();
                sorting.Remove(solvedItem);

                output.Add(solvedItem);

                foreach (var dependency in dependencies.Where(it => it.TargetID.Equals(solvedItem.AppliedID)).ToList())
                {
                    dependencies.Remove(dependency);
                    sorting.Add(dependency);
                }
            }

            return !dependencies.Any();
        }
    }
}
