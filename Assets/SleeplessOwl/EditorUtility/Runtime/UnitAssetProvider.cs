using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public interface IUnitAssetProvide
{
    public Sprite HeadIcon { get; }

    public GameObject Model{ get; }


}

public class UnitAssetProvider : MonoBehaviour, IUnitAssetProvide
{
    public Sprite m_headIcon;

    Sprite IUnitAssetProvide.HeadIcon => m_headIcon;
    GameObject IUnitAssetProvide.Model => this.gameObject;

}


public static class NameOf<TSource>
{
    #region Public Methods

    public static string Full(Expression<Func<TSource, object>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression != null && unaryExpression.NodeType == ExpressionType.Convert)
                memberExpression = unaryExpression.Operand as MemberExpression;
        }

        var result = memberExpression.ToString();
        result = result.Substring(result.IndexOf('.') + 1);

        return result;
    }

    public static string Full(string sourceFieldName, Expression<Func<TSource, object>> expression)
    {
        var result = Full(expression);
        result = string.IsNullOrEmpty(sourceFieldName) ? result : sourceFieldName + "." + result;
        return result;
    }

    #endregion
}
