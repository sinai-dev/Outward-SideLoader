using System.Collections.Generic;
using System.Linq;

namespace SideLoader.Model
{
    public static class TemplateDependancySolver
    {
        public static List<IContentTemplate> SolveDependencies(List<IContentTemplate> allTemplates)
        {
            if (allTemplates == null || !allTemplates.Any())
                return allTemplates;

            List<IContentTemplate> sorted;

            // Check if there are any actual dependencies
            var dependencies = allTemplates.Where(it => !it.DoesTargetExist).ToList();

            if (dependencies.Any())
            {
                // Toplogical sort dependencies and full list.
                if (!TopologicalSort(allTemplates, dependencies, out List<IContentTemplate> resolved))
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

            return sorted;
        }

        internal static bool TopologicalSort(List<IContentTemplate> allTemplates, List<IContentTemplate> dependencies, out List<IContentTemplate> output)
        {
            output = new List<IContentTemplate>();

            var graph = new HashSet<IContentTemplate>(allTemplates.Where(it => it.DoesTargetExist));

            while (graph.Any())
            {
                var outgoing = graph.First();

                graph.Remove(outgoing);
                output.Add(outgoing);

                foreach (var incoming in dependencies.Where(it => outgoing.IsDependancyOf(it)).ToList())
                {
                    // in this design, each node can have only one edge, so we dont need to check for more dependencies.
                    dependencies.Remove(incoming);
                    graph.Add(incoming);
                }
            }

            return !dependencies.Any();
        }

        internal static bool IsDependancyOf(this IContentTemplate outgoing, IContentTemplate incoming)
        {
            var type = outgoing.AppliedID?.GetType();
            if (type == null)
                return false;

            return typeof(string).IsAssignableFrom(type)
                ? (string)outgoing.AppliedID == (string)incoming.TargetID
                : (int)outgoing.AppliedID == (int)incoming.TargetID;
        }
    }
}
