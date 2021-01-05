using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SideLoader.Model
{
    public class TemplateDependancySolver<T, U> where T : IPrefabTemplate<U>
    {
        public void ApplyTemplates(List<T> list)
        {
            if (list == null || !list.Any())
                return;

            // first, pick templates where the target ID already exists

            var sorted = list.Where(it => it.DoesTargetExist).ToList();

            // then from remaining, sort to resolve dependancies on custom items

            var unsorted = list.Where(it => !it.DoesTargetExist).ToList();

            if (unsorted.Count > 0)
            {
                SL.Log("Sorting " + unsorted.Count + " unresolved template dependencies...");

                for (int i = 0; i < unsorted.Count; i++)
                {
                    var template = unsorted[i];

                    //SL.Log(i + ", targeting " + template.TargetID);

                    // if there is a sorted template that creates this ID,
                    // then just add this template to the end of the list.
                    if (sorted.Any(it => it.NewID.Equals(template.TargetID)))
                    {
                        sorted.Add(template);
                        unsorted.Remove(template);
                        i--;
                    }
                    else
                    {
                        // there is an unsorted template that applies this target ID.
                        // push this template to the end of the unsorted list.
                        if (!template.TargetID.Equals(template.NewID) 
                            && unsorted.Any(it => it.NewID.Equals(template.TargetID) && !it.TargetID.Equals(template.NewID)))
                        {
                            unsorted.Remove(template);
                            unsorted.Add(template);
                            i--;
                        }
                        else
                        {
                            // there is no non-circular template applying the target ID.
                            SL.LogWarning("Unable to find any valid target ID for '" + template.TargetID + "!"
                                + "Make sure you set a valid ID, and that it is not a circular dependency.");
                            unsorted.Remove(template);
                            i--;
                        }
                    }
                }
            }

            // then from remaining, simply apply templates.
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
    }
}
