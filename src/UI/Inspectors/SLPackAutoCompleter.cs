using SideLoader.Inspectors;
using SideLoader.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SideLoader.Inspectors
{
    public struct ContentSuggestion
    {
        public object UnderlyingValue;
        public string DisplayedValue;
        public string SearchQueryValue;
        public string IDValue;

        public ContentSuggestion(object value, string displayed, string searchQuery, string idValue)
        {
            this.UnderlyingValue = value;
            this.DisplayedValue = displayed;
            this.SearchQueryValue = searchQuery;
            this.IDValue = idValue;
        }
    }

    public class AutoCompleteInputField : InputField
    {
        public override void OnSelect(BaseEventData eventData)
        {
            if (m_selectCoroutine != null)
            {
                StopCoroutine(m_selectCoroutine);
                m_selectCoroutine = null;
            }

            base.OnSelect(eventData);

            SLPackListView.Instance.UpdateAutocompletes();

            m_selectCoroutine = StartCoroutine(SelectCoroutine());
        }

        private Coroutine m_selectCoroutine;
        private bool m_wasDeselected;

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            m_wasDeselected = true;
        }

        private IEnumerator SelectCoroutine()
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.5f);

            float timeOffAutocompleter = 0f;
            while (timeOffAutocompleter < 0.1f)
            {
                yield return new WaitForEndOfFrame();

                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    var data = new PointerEventData(null) { position = Input.mousePosition };
                    var casters = Resources.FindObjectsOfTypeAll<GraphicRaycaster>();
                    var list = new List<RaycastResult>();
                    bool forceBreak = false;
                    bool anyHits = false;
                    foreach (var caster in casters)
                    {
                        if (list.Any())
                            list.Clear();
                        caster.Raycast(data, list);
                        if (list.Any(it => it.gameObject))
                        {
                            anyHits = true;
                            var result = list.Where(it => it.gameObject).First();
                            if (!result.gameObject.transform.GetGameObjectPath().Contains("AutoCompleter"))
                            {
                                forceBreak = true;
                                break;
                            }
                        }
                    }
                    if (forceBreak || !anyHits)
                        break;
                }
                
                if (m_wasDeselected)
                {
                    timeOffAutocompleter += Time.deltaTime;

                    var obj = EventSystem.current.currentSelectedGameObject;
                    if (obj)
                    {
                        var path = obj.transform.GetGameObjectPath();
                        if (path.Contains("AutoCompleter"))
                            timeOffAutocompleter = 0f;
                    }
                }
            }

            SLPackListView.Instance.AutoCompleter.m_mainObj.SetActive(false);
            m_selectCoroutine = null;
        }
    }

    public class ContentAutoCompleter
    {
        //// ~~~~ Static ~~~~

        internal static Dictionary<Type, List<ContentSuggestion>> s_typeToOptionsDict = new Dictionary<Type, List<ContentSuggestion>>();
        internal List<ContentSuggestion> m_availableOptions;

        public ContentAutoCompleter Instance;

        public GameObject m_mainObj;

        private readonly List<GameObject> m_suggestionButtons = new List<GameObject>();
        private readonly List<Text> m_suggestionTexts = new List<Text>();

        private bool m_suggestionsDirty;
        private ContentSuggestion[] m_suggestions = new ContentSuggestion[0];

        //private string m_prevInput = null;

        public void Init()
        {
            ConstructUI();

            m_mainObj.SetActive(false);
        }

        public void Update()
        {
            if (!m_mainObj)
                return;

            RefreshButtons();
            UpdatePosition();
        }

        public void SetAutocompletes(ContentSuggestion[] suggestions)
        {
            m_suggestions = suggestions;
            m_suggestionsDirty = true;
        }

        public void ClearAutocompletes()
        {
            m_suggestions = new ContentSuggestion[0];
            m_suggestionsDirty = true;
        }

        public void CheckAutocomplete()
        {
            var genInput = SLPackListView.Instance.m_generatorTargetInput;
            string input = genInput.text.Trim();

            var genType = SLPackListView.Instance.m_currentGeneratorType;
            var gameType = Serializer.GetGameType(genType);

            if (!s_typeToOptionsDict.ContainsKey(gameType))
            {
                s_typeToOptionsDict.Add(gameType, new List<ContentSuggestion>());
                var list = s_typeToOptionsDict[gameType];

                if (typeof(UnityEngine.Object).IsAssignableFrom(gameType))
                {
                    HashSet<string> checkedObjects = new HashSet<string>();

                    foreach (var obj in Resources.FindObjectsOfTypeAll(gameType))
                    {
                        var suggest = new ContentSuggestion
                        {
                            DisplayedValue = obj.name,
                            UnderlyingValue = obj
                        };

                        if (obj is Item item)
                        {
                            suggest.SearchQueryValue = $"{item.ItemID} {item.Name} {item.name}".ToLower();
                            suggest.IDValue = item.ItemID.ToString();
                            suggest.DisplayedValue = $"{item.Name} ({suggest.DisplayedValue})";
                        }
                        else if (obj is StatusEffect status)
                        {
                            suggest.SearchQueryValue = $"{status.IdentifierName} {status.StatusName} {status.name}".ToLower();
                            suggest.IDValue = status.IdentifierName.ToString();
                            suggest.DisplayedValue = $"{status.StatusName} ({suggest.DisplayedValue})";
                        }
                        else if (obj is ImbueEffectPreset imbue)
                        {
                            suggest.SearchQueryValue = $"{imbue.PresetID} {imbue.Name} {imbue.name}".ToLower();
                            suggest.IDValue = imbue.PresetID.ToString();
                            suggest.DisplayedValue = $"{imbue.Name} ({suggest.DisplayedValue})";
                        }
                        else if (obj is Recipe recipe)
                        {
                            suggest.SearchQueryValue = $"{recipe.UID} {recipe.Name} {recipe.name}".ToLower();
                            suggest.IDValue = recipe.UID;
                        }
                        else if (obj is EnchantmentRecipe enchantRecipe)
                        {
                            suggest.SearchQueryValue = $"{enchantRecipe.RecipeID} {enchantRecipe.name}".ToLower();
                            suggest.IDValue = enchantRecipe.ResultID.ToString();
                        }
                        else
                            continue;

                        if (checkedObjects.Contains(suggest.DisplayedValue))
                            continue;

                        checkedObjects.Add(suggest.DisplayedValue);
                        list.Add(suggest);
                    }
                }
            }

            m_availableOptions = s_typeToOptionsDict[gameType];

            if (!string.IsNullOrEmpty(input))
                SetAutocompletes(GetAutocompletes(input));
            else
                SetAutocompletes(m_availableOptions.ToArray());

            //m_prevInput = input;
        }

        public ContentSuggestion[] GetAutocompletes(string input)
        {
            var options = m_availableOptions;

            var search = input.ToLower();
            var results = options.Where(it => it.SearchQueryValue.Contains(search));

            return results.ToArray();
        }

        #region UI Construction

        private void RefreshButtons()
        {
            if (!m_suggestionsDirty)
                return;

            if (m_suggestions.Length < 1)
            {
                if (m_mainObj.activeSelf)
                    m_mainObj?.SetActive(false);

                return;
            }

            if (!m_mainObj.activeSelf)
                m_mainObj.SetActive(true);

            if (m_suggestions.Length < 1)
            {
                m_suggestionsDirty = false;
                return;
            }

            for (int i = 0; i < m_suggestionButtons.Count; i++)
            {
                if (i >= m_suggestions.Length)
                {
                    if (m_suggestionButtons[i].activeSelf)
                        m_suggestionButtons[i].SetActive(false);
                }
                else
                {
                    if (!m_suggestionButtons[i].activeSelf)
                        m_suggestionButtons[i].SetActive(true);

                    var suggestion = m_suggestions[i];
                    var label = m_suggestionTexts[i];

                    label.text = suggestion.DisplayedValue;
                }
            }
        }

        private void UpdatePosition()
        {
            try
            {
                var m_genInput = SLPackListView.Instance.m_generatorTargetInput;

                if (!m_genInput)
                    return;

                var textGen = m_genInput.textComponent.cachedTextGenerator;

                var pos = textGen.characters[0].cursorPos;

                pos = m_genInput.transform.TransformPoint(pos);

                m_mainObj.transform.position = new Vector3(pos.x + 10, pos.y - 20, 0);
            }
            catch //(Exception e)
            {
                //ExplorerCore.Log(e.ToString());
            }
        }

        private void ConstructUI()
        {
            var parent = UIManager.CanvasRoot;

            var obj = UIFactory.CreateScrollView(parent, out GameObject content, out _, new Color(0.1f, 0.1f, 0.1f, 1));
            obj.name = "AutoCompleterObject";
            m_mainObj = obj;

            var mainRect = obj.GetComponent<RectTransform>();
            //m_thisRect = mainRect;
            mainRect.pivot = new Vector2(0f, 1f);
            mainRect.anchorMin = new Vector2(0.45f, 0.45f);
            mainRect.anchorMax = new Vector2(0.65f, 0.6f);
            mainRect.offsetMin = Vector2.zero;
            mainRect.offsetMax = Vector2.zero;

            var mainGroup = content.GetComponent<VerticalLayoutGroup>();
            mainGroup.childControlHeight = false;
            mainGroup.childControlWidth = true;
            mainGroup.childForceExpandHeight = false;
            mainGroup.childForceExpandWidth = true;

            for (int i = 0; i < 100; i++)
            {
                var buttonObj = UIFactory.CreateButton(content);
                Button btn = buttonObj.GetComponent<Button>();
                ColorBlock btnColors = btn.colors;
                btnColors.normalColor = new Color(0f, 0f, 0f, 0f);
                btnColors.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
                btn.colors = btnColors;

                var nav = btn.navigation;
                nav.mode = Navigation.Mode.Vertical;
                btn.navigation = nav;

                var btnLayout = buttonObj.AddComponent<LayoutElement>();
                btnLayout.minHeight = 20;

                var text = btn.GetComponentInChildren<Text>();
                text.alignment = TextAnchor.MiddleLeft;
                text.color = Color.white;

                //var hiddenChild = UIFactory.CreateUIObject("HiddenText", buttonObj);
                //hiddenChild.SetActive(false);
                //var hiddenText = hiddenChild.AddComponent<Text>();
                //m_hiddenSuggestionTexts.Add(hiddenText);

                int thisIndex = i;
                btn.onClick.AddListener(UseAutocompleteButton);

                void UseAutocompleteButton()
                {
                    var suggestion = m_suggestions[thisIndex];
                    SLPackListView.Instance.UseAutocomplete(suggestion.IDValue);
                }

                m_suggestionButtons.Add(buttonObj);
                m_suggestionTexts.Add(text);
            }
        }

        #endregion
    }
}
