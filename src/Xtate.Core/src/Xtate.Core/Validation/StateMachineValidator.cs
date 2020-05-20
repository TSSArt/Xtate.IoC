﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace TSSArt.StateMachine
{
	public class StateMachineValidator : IStateMachineValidator
	{
		public static readonly IStateMachineValidator Instance = new StateMachineValidator();

	#region Interface IStateMachineValidator

		public void Validate(IStateMachine stateMachine, IErrorProcessor errorProcessor)
		{
			new Validator(errorProcessor).Validate(stateMachine);
		}

	#endregion

		private class Validator : StateMachineVisitor
		{
			private readonly IErrorProcessor _errorProcessor;

			public Validator(IErrorProcessor errorProcessor) => _errorProcessor = errorProcessor;

			public void Validate(IStateMachine stateMachine)
			{
				Visit(ref stateMachine);
			}

			private void AddError(object entity, string message) => _errorProcessor.AddError<StateMachineValidator>(entity, message);

			protected override void Visit(ref IAssign entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Location == null)
				{
					AddError(entity, Resources.ErrorMessage_AssignItemLocationMissed);
				}

				if (entity.Expression == null && entity.InlineContent == null)
				{
					AddError(entity, Resources.ErrorMessage_AssignItemContentAndExpressionMissed);
				}

				if (entity.Expression != null && entity.InlineContent != null)
				{
					AddError(entity, Resources.ErrorMessage_AssignItemContentAndExpressionSpecified);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ICancel entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.SendId == null && entity.SendIdExpression == null)
				{
					AddError(entity, Resources.ErrorMessage_CancelItemSendIdAndExpressionMissed);
				}

				if (entity.SendId != null && entity.SendIdExpression != null)
				{
					AddError(entity, Resources.ErrorMessage_CancelItemSendIdAndExpressionSpecified);
				}

				if (entity.SendId != null && entity.SendId.Length == 0)
				{
					AddError(entity, Resources.ErrorMessage_SendidAttributeCantBeEmptyInCancelElement);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IConditionExpression entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression == null)
				{
					AddError(entity, Resources.ErrorMessage_ConditionExpressionCantBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ILocationExpression entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression == null)
				{
					AddError(entity, Resources.ErrorMessage_Location_expression_can_t_be_null);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IScriptExpression entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression == null)
				{
					AddError(entity, Resources.ErrorMessage_Script_expression_can_t_be_null);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IContentBody entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Value == null)
				{
					AddError(entity, Resources.ErrorMessage_ContentValueCantBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IContent entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Expression == null && entity.Body == null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionAndBodyMissedInContent);
				}

				if (entity.Expression != null && entity.Body != null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionAndBodySpecifiedInContent);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ICustomAction entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Xml == null)
				{
					AddError(entity, Resources.ErrorMessage_XmlCannotBeNull);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IData entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (string.IsNullOrEmpty(entity.Id))
				{
					AddError(entity, Resources.ErrorMessage_Id_property_required_in_Data_element);
				}

				if (entity.InlineContent != null && entity.Expression != null || entity.InlineContent != null && entity.Source != null || entity.Source != null && entity.Expression != null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionSourceInData);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IElseIf entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Condition == null)
				{
					AddError(entity, Resources.ErrorMessage_ConditionRequiredForElseIf);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IFinalize entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				foreach (var executableEntity in entity.Action)
				{
					if (executableEntity is IRaise)
					{
						AddError(executableEntity, Resources.ErrorMessage_Raise_can_t_be_used_in_Finalize_element);
					}

					if (executableEntity is ISend)
					{
						AddError(executableEntity, Resources.ErrorMessage_Send_can_t_be_used_in_Finalize_element);
					}
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IForEach entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Array == null)
				{
					AddError(entity, Resources.ErrorMessage_ArrayPropertyRequiredForForEach);
				}

				if (entity.Item == null)
				{
					AddError(entity, Resources.ErrorMessage_ConditionRequiredForForEach);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IHistory entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Transition == null)
				{
					AddError(entity, Resources.ErrorMessage_Transition_must_be_present_in_History_element);
				}

				if (entity.Type < HistoryType.Shallow || entity.Type > HistoryType.Deep)
				{
					AddError(entity, Resources.ErrorMessage_Invalid_Type_value_in_History_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IIf entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Condition == null)
				{
					AddError(entity, Resources.ErrorMessage_onditionRequiredForIf);
				}

				var condition = true;

				foreach (var op in entity.Action)
				{
					switch (op)
					{
						case IElseIf _:
							if (!condition)
							{
								AddError(op, Resources.ErrorMessage_ElseifCannotFollowElse);
							}

							break;

						case IElse _:
							if (!condition)
							{
								AddError(op, Resources.ErrorMessage_ElseCanBeUsedOnlyOnce);
							}

							condition = false;
							break;
					}
				}


				base.Visit(ref entity);
			}

			protected override void Visit(ref IInitial entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Transition == null)
				{
					AddError(entity, Resources.ErrorMessage_Transition_must_be_present_in_Initial_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IInvoke entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Type == null && entity.TypeExpression == null)
				{
					AddError(entity, Resources.ErrorMessage_Type_or_TypeExpression_must_be_specified_in_Invoke_element);
				}

				if (entity.Type != null && entity.TypeExpression != null)
				{
					AddError(entity, Resources.ErrorMessage_Type_and_TypeExpression_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				if (entity.Id != null && entity.IdLocation != null)
				{
					AddError(entity, Resources.ErrorMessage_Id_and_IdLocation_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				if (entity.Source != null && entity.SourceExpression != null)
				{
					AddError(entity, Resources.ErrorMessage_Source_and_SourceExpression_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				if (!entity.NameList.IsDefaultOrEmpty && !entity.Parameters.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_NameList_and_Parameters_can_t_be_used_at_the_same_time_in_Invoke_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IParam entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Name == null)
				{
					AddError(entity, Resources.ErrorMessage_Name_attributes_required_in_Param_element);
				}

				if (entity.Expression != null && entity.Location != null)
				{
					AddError(entity, Resources.ErrorMessage_ExpressionLocationInParam);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IRaise entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.OutgoingEvent == null)
				{
					AddError(entity, Resources.ErrorMessage_EventRequiredForRaise);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IScript entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Source != null && entity.Content != null)
				{
					AddError(entity, Resources.ErrorMessage_Source_and_Body_can_t_be_used_at_the_same_time_in_Assign_element);
				}

				base.Visit(ref entity);
			}

			[SuppressMessage(category: "ReSharper", checkId: "CyclomaticComplexity", Justification = "OK")]
			protected override void Visit(ref ISend entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.EventName != null && entity.EventExpression != null || entity.EventName != null && entity.Content != null || entity.EventExpression != null && entity.Content != null)
				{
					AddError(entity, Resources.ErrorMessage_EvenExpressionContentInSend);
				}

				if (entity.Target != null && entity.TargetExpression != null)
				{
					AddError(entity, Resources.ErrorMessage_Target_and_TargetExpression_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.Type != null && entity.TypeExpression != null)
				{
					AddError(entity, Resources.ErrorMessage_Type_and_TypeExpression_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.Id != null && entity.IdLocation != null)
				{
					AddError(entity, Resources.ErrorMessage_Id_and_IdLocation_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.DelayMs != null && entity.DelayExpression != null)
				{
					AddError(entity, Resources.ErrorMessage_EventExpressionInSend);
				}

				if (!entity.NameList.IsDefaultOrEmpty && entity.Content != null)
				{
					AddError(entity, Resources.ErrorMessage_NameList_and_Content_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (!entity.Parameters.IsDefaultOrEmpty && entity.Content != null)
				{
					AddError(entity, Resources.ErrorMessage_Parameters_and_Content_can_t_be_used_at_the_same_time_in_Send_element);
				}

				if (entity.EventName == null && entity.EventExpression == null && entity.Content == null)
				{
					AddError(entity, Resources.ErrorMessage_Must_be_present_Event_or_EventExpression_or_Content_in_Send_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IStateMachine entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Initial != null && entity.States.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_Initial_state_property_cannot_be_used_without_any_states);
				}

				if (entity.Binding < BindingType.Early || entity.Binding > BindingType.Late)
				{
					AddError(entity, Resources.ErrorMessage_Invalid_BindingType_value_in_StateMachine_element);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref IState entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.Initial != null && entity.States.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_Initial_state_property_can_be_used_only_in_complex_states);
				}

				base.Visit(ref entity);
			}

			protected override void Visit(ref ITransition entity)
			{
				if (entity == null) throw new ArgumentNullException(nameof(entity));

				if (entity.EventDescriptors.IsDefaultOrEmpty && entity.Condition == null && entity.Target.IsDefaultOrEmpty)
				{
					AddError(entity, Resources.ErrorMessage_Must_be_present_at_least_Event_or_Condition_or_Target_in_Transition_element);
				}

				base.Visit(ref entity);
			}
		}
	}
}