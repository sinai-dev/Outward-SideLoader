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

        // Credit: https://gist.github.com/Sup3rc4l1fr4g1l1571c3xp14l1d0c10u5/3341dba6a53d7171fe3397d13d00ee3f
        // (modified)
        internal static bool TopologicalSort(List<T> allTemplates, List<T> dependencies, out List<T> output)
        {
            // Empty list that will contain the sorted elements
            output = new List<T>();

            // Set of all nodes with no incoming edges
            var sorting = new HashSet<T>(allTemplates.Where(it => it.DoesTargetExist));

            // while S is non-empty do
            while (sorting.Any())
            {
                // remove a node n from S
                var solvedItem = sorting.First();
                sorting.Remove(solvedItem);

                //SL.Log("Outputting template, targets " + solvedItem.TargetID + ", applied id " + solvedItem.AppliedID);

                // add n to tail of L
                output.Add(solvedItem);

                // for each node m with an edge e from n to m do
                foreach (var dependency in dependencies.Where(it => it.TargetID.Equals(solvedItem.AppliedID)).ToList())
                {
                    // remove edge e from the graph
                    dependencies.Remove(dependency);

                    // if m has no other incoming edges then
                    if (dependencies.All(it => !ReferenceEquals(it, dependency)))
                    {
                        //SL.Log("No other dependency for this template, adding to sorting list");
                        // insert m into S
                        sorting.Add(dependency);
                    }
                }
            }

            // if graph has edges then
            if (dependencies.Any())
                return false;
            else
                return true;
        }
    }
}
