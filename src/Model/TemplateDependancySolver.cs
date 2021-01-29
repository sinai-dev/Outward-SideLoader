using System.Collections.Generic;
using System.Linq;

namespace SideLoader.Model
{
    public static class TemplateDependancySolver
    {
        public static List<ContentTemplate> SolveDependencies(List<ContentTemplate> allTemplates)
        {
            if (allTemplates == null || !allTemplates.Any())
                return allTemplates;

            List<ContentTemplate> sorted;

            // Check if there are any actual dependencies
            var dependencies = allTemplates.Where(it => !it.DoesTargetExist).ToList();

            if (dependencies.Any())
            {
                // Toplogical sort dependencies and full list.
                if (!TopologicalSort(allTemplates, dependencies, out List<ContentTemplate> resolved))
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

        internal static bool TopologicalSort(List<ContentTemplate> allTemplates, List<ContentTemplate> dependencies, out List<ContentTemplate> output)
        {
            output = new List<ContentTemplate>();

            var graph = new HashSet<ContentTemplate>(allTemplates.Where(it => it.DoesTargetExist));

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

        internal static bool IsDependancyOf(this ContentTemplate outgoing, ContentTemplate incoming)
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
