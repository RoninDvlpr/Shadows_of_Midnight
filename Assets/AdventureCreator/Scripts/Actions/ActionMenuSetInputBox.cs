/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2023
 *	
 *	"ActionMenuSetInputBox.cs"
 * 
 *	This action replaces the text within an Input box element.
 * 
 */

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionMenuSetInputBox : Action, IMenuReferencer
	{
		
		public string menuName;
		public int menuNameParameterID = -1;
		public string elementName;
		public int elementNameParameterID = -1;

		public string newLabel;
		private string runtimeNewLabel;
		public int newLabelParameterID = -1;
		public int newLabelLineID = -1;
		private int runtimeLineID;

		public enum SetMenuInputBoxSource { EnteredHere, FromGlobalVariable };
		public SetMenuInputBoxSource setMenuInputBoxSource = SetMenuInputBoxSource.EnteredHere;

		private enum ElementContentType { Text=0, Texture=1 };
		[SerializeField] private ElementContentType elementContentType = ElementContentType.Text;

		public int varID = 0;
		public int varParameterID = -1;

		public Texture newTexture;
		public int newTextureParameterID = -1;
		private Texture runtimeNewTexture;


		public override ActionCategory Category { get { return ActionCategory.Menu; }}
		public override string Title { get { return "Update content"; }}
		public override string Description { get { return "Replaces the text within an element."; }}


		public override void AssignValues (List<ActionParameter> parameters)
		{
			menuName = AssignString (parameters, menuNameParameterID, menuName);
			elementName = AssignString (parameters, elementNameParameterID, elementName);

			switch (elementContentType)
			{
				case ElementContentType.Text:
					{
						runtimeNewLabel = AssignString (parameters, newLabelParameterID, newLabel);
						varID = AssignVariableID (parameters, varParameterID, varID);
						runtimeLineID = newLabelLineID;
						if (newLabelParameterID >= 0)
						{
							ActionParameter parameter = GetParameterWithID (parameters, newLabelParameterID);
							if (parameter != null && parameter.parameterType == ParameterType.InventoryItem)
							{
								newLabelLineID = -1;
								InvItem invItem = KickStarter.inventoryManager.GetItem (parameter.intValue);
								if (invItem != null)
								{
									runtimeLineID = invItem.lineID;
								}
							}
							else if (parameter != null && parameter.parameterType == ParameterType.Document)
							{
								newLabelLineID = -1;
								Document document = KickStarter.inventoryManager.GetDocument (parameter.intValue);
								if (document != null)
								{
									runtimeLineID = document.titleLineID;
								}
							}
						}
					}
					break;

				case ElementContentType.Texture:
					{
						runtimeNewTexture = (Texture) AssignObject<Texture> (parameters, newTextureParameterID, newTexture);

						if (newTextureParameterID >= 0)
						{
							ActionParameter parameter = GetParameterWithID (parameters, newTextureParameterID);
							if (parameter != null && parameter.parameterType == ParameterType.InventoryItem)
							{
								int itemID = parameter.intValue;
								InvItem invItem = KickStarter.inventoryManager.GetItem (itemID);
								if (invItem != null)
								{
									runtimeNewTexture = invItem.tex;
								}
							}
							else if (parameter != null && parameter.parameterType == ParameterType.Document)
							{
								int documentID = parameter.intValue;
								Document document = KickStarter.inventoryManager.GetDocument (documentID);
								if (document != null)
								{
									runtimeNewTexture = document.texture;
								}
							}
						}
					}
					break;

				default:
					break;
			}
		}

		
		public override float Run ()
		{
			if (!string.IsNullOrEmpty (menuName) && !string.IsNullOrEmpty (elementName))
			{
				MenuElement menuElement = PlayerMenus.GetElementWithName (menuName, elementName);
				if (menuElement != null)
				{
					switch (elementContentType)
					{
						case ElementContentType.Text:
							{
								if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
								{
									GVar variable = GlobalVariables.GetVariable (varID);
									if (variable != null)
									{
										menuElement.OverrideLabel (variable.TextValue, variable.canTranslate ? variable.textValLineID : -1);
									}
								}
								else
								{
									menuElement.OverrideLabel (runtimeNewLabel, runtimeLineID);
								}
							}
							break;

						case ElementContentType.Texture:
							{
								if (runtimeNewTexture)
								{
									MenuGraphic menuGraphic = menuElement as MenuGraphic;
									if (menuGraphic)
									{
										menuGraphic.SetNormalGraphicTexture (runtimeNewTexture);
									}
									else if (menuGraphic.ParentMenu && menuGraphic.ParentMenu.IsUnityUI ())
									{
										LogWarning ("Only 'AC' Menus and Graphic elements can have their texture updated at runtime");
									}
									else
									{
										menuElement.backgroundTexture = (Texture2D) runtimeNewTexture;
									}
								}
							}
							break;

						default:
							break;
					}
				}
				else
				{
					LogWarning ("Cannot find Element '" + elementName + "' within Menu '" + menuName + "'");
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (List<ActionParameter> parameters)
		{
			menuNameParameterID = Action.ChooseParameterGUI ("Menu containing Element:", parameters, menuNameParameterID, new ParameterType[2] { ParameterType.String, ParameterType.PopUp });
			if (menuNameParameterID < 0)
			{
				menuName = TextField ("Menu containing Element:", menuName);
			}
			
			elementNameParameterID = Action.ChooseParameterGUI ("Element name:", parameters, elementNameParameterID, new ParameterType[2] { ParameterType.String, ParameterType.PopUp });
			if (elementNameParameterID < 0)
			{
				elementName = TextField ("Element name:", elementName);
			}

			elementContentType = (ElementContentType) EditorGUILayout.EnumPopup ("Content type:", elementContentType);
			switch (elementContentType)
			{
				case ElementContentType.Text:
					{
						setMenuInputBoxSource = (SetMenuInputBoxSource) EditorGUILayout.EnumPopup ("New label is:", setMenuInputBoxSource);
						if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
						{
							newLabelParameterID = Action.ChooseParameterGUI ("New label:", parameters, newLabelParameterID, new ParameterType[4] { ParameterType.String, ParameterType.PopUp, ParameterType.InventoryItem, ParameterType.Document });
							if (newLabelParameterID < 0)
							{
								newLabel = TextField ("New label:", newLabel);
							}
						}
						else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
						{
							varParameterID = Action.ChooseParameterGUI ("String variable:", parameters, varParameterID, new ParameterType[2] { ParameterType.String, ParameterType.PopUp });
							if (varParameterID == -1)
							{
								varID = AdvGame.GlobalVariableGUI ("String variable:", varID, new VariableType[2] { VariableType.String, VariableType.PopUp });
							}
						}
					}
					break;

				case ElementContentType.Texture:
					newTextureParameterID = Action.ChooseParameterGUI ("New texture:", parameters, newTextureParameterID, new ParameterType[3] { ParameterType.UnityObject, ParameterType.InventoryItem, ParameterType.Document });
					if (newTextureParameterID < 0)
					{
						newTexture = (Texture) EditorGUILayout.ObjectField ("New texture:", newTexture, typeof (Texture), true);
					}

					if (newTextureParameterID >= 0 || newTexture)
					{
						EditorGUILayout.HelpBox ("A texture can only be assigned to either Graphic elements, or as the background of an AC menu-based element.", MessageType.Info);
					}
					break;

				default:
					break;
			}
		}
		
		
		public override string SetLabel ()
		{
			if (!string.IsNullOrEmpty (elementName))
			{
				string labelAdd = elementName + " - ";
				if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
				{
					labelAdd += "'" + newLabel + "'";
				}
				else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
				{
					labelAdd += "from Variable";
				}
				return labelAdd;
			}
			return string.Empty;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			string updatedLabel = AdvGame.ConvertLocalVariableTokenToGlobal (newLabel, oldLocalID, newGlobalID);
			if (newLabel != updatedLabel)
			{
				wasAmended = true;
				newLabel = updatedLabel;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool isAffected = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			string updatedLabel = AdvGame.ConvertGlobalVariableTokenToLocal (newLabel, oldGlobalID, newLocalID);
			if (newLabel != updatedLabel)
			{
				isAffected = true;
				if (isCorrectScene)
				{
					newLabel = updatedLabel;
				}
			}
			return isAffected;
		}


		public override int GetNumVariableReferences (VariableLocation location, int _varID, List<ActionParameter> parameters, Variables _variables = null, int _variablesConstantID = 0)
		{
			int thisCount = 0;

			if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
			{
				string tokenText = AdvGame.GetVariableTokenText (location, _varID, _variablesConstantID);
				if (!string.IsNullOrEmpty (tokenText) && newLabel.ToLower ().Contains (tokenText))
				{
					thisCount ++;
				}
			}
			else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
			{
				if (location == VariableLocation.Global && varID == _varID && varParameterID < 0)
				{
					thisCount ++;
				}
			}

			thisCount += base.GetNumVariableReferences (location, varID, parameters, _variables, _variablesConstantID);
			return thisCount;
		}


		public override int UpdateVariableReferences (VariableLocation location, int oldVarID, int newVarID, List<ActionParameter> parameters, Variables _variables = null, int _variablesConstantID = 0)
		{
			int thisCount = 0;

			if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
			{
				string oldTokenText = AdvGame.GetVariableTokenText (location, oldVarID, _variablesConstantID);
				if (!string.IsNullOrEmpty (oldTokenText) && newLabel.ToLower ().Contains (oldTokenText))
				{
					string newTokenText = AdvGame.GetVariableTokenText (location, newVarID, _variablesConstantID);
					newLabel = newLabel.Replace (oldTokenText, newTokenText);
					thisCount++;
				}
			}
			else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
			{
				if (location == VariableLocation.Global && varID == oldVarID && varParameterID < 0)
				{
					varID = newVarID;
					thisCount++;
				}
			}

			thisCount += base.UpdateVariableReferences (location, oldVarID, newVarID, parameters, _variables, _variablesConstantID);
			return thisCount;
		}


		public int GetNumMenuReferences (string _menuName, string _elementName = "")
		{
			if (menuNameParameterID < 0 && menuName == _menuName)
			{
				if (string.IsNullOrEmpty (elementName))
				{
					return 1;
				}

				if (elementNameParameterID < 0 && _elementName == elementName)
				{
					return 1;
				}
			}

			return 0;
		}

		#endif


		#region ITranslatable

		public string GetTranslatableString (int index)
		{
			return newLabel;
		}


		public int GetTranslationID (int index)
		{
			return newLabelLineID;
		}


		#if UNITY_EDITOR

		public void UpdateTranslatableString (int index, string updatedText)
		{
			newLabel = updatedText;
		}


		public int GetNumTranslatables ()
		{
			return 1;
		}


		public bool HasExistingTranslation (int index)
		{
			return newLabelLineID > -1;
		}


		public void SetTranslationID (int index, int _lineID)
		{
			newLabelLineID = _lineID;
		}


		public string GetOwner (int index)
		{
			return string.Empty;
		}


		public bool OwnerIsPlayer (int index)
		{
			return false;
		}


		public AC_TextType GetTranslationType (int index)
		{
			return AC_TextType.MenuElement;
		}


		public bool CanTranslate (int index)
		{
			return setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere && newLabelParameterID < 0 && !string.IsNullOrEmpty (newLabel);
		}

		#endif

		#endregion


		/**
		 * <summary>Creates a new instance of the 'Menu: Update content' Action, set to update an InputBox element directly</summary>
		 * <param name = "menuName">The name of the Menu containing the element</param>
		 * <param name = "inputBoxElementName">The name of the element</param>
		 * <param name = "newText">The new text to display in the InputBox element</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionMenuSetInputBox CreateNew_SetDirectly (string menuName, string elementName, string newText)
		{
			ActionMenuSetInputBox newAction = CreateNew<ActionMenuSetInputBox> ();
			newAction.menuName = menuName;
			newAction.elementName = elementName;
			newAction.newLabel = newText;
			return newAction;
		}


		/**
		 * <summary>Creates a new instance of the 'Menu: Update content' Action, set to update an InputBox element from a Global String variable</summary>
		 * <param name = "menuName">The name of the Menu containing the element</param>
		 * <param name = "inputBoxElementName">The name of the element</param>
		 * <param name = "globalStringVariableID">The ID number of the Global String variable with the InputBox element's new display text</param>
		 * <returns>The generated Action</returns>
		 */
		public static ActionMenuSetInputBox CreateNew_SetFromVariable (string menuName, string elementName, int globalStringVariableID)
		{
			ActionMenuSetInputBox newAction = CreateNew<ActionMenuSetInputBox> ();
			newAction.menuName = menuName;
			newAction.elementName = elementName;
			newAction.varID = globalStringVariableID;
			return newAction;
		}
		
	}
	
}