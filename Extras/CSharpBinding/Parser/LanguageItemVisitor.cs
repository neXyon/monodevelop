// created on 22.08.2003 at 19:02

using System;
using System.Collections;

using ICSharpCode.SharpRefactory.Parser;
using ICSharpCode.SharpRefactory.Parser.AST;
using CSharpBinding.Parser.SharpDevelopTree;

using MonoDevelop.Projects.Parser;

namespace CSharpBinding.Parser
{
	internal class LanguageItemVisitor : AbstractASTVisitor
	{
		Resolver resolver;
		
		internal LanguageItemVisitor (Resolver resolver)
		{
			this.resolver = resolver;
		}
		
		public override object Visit(PrimitiveExpression primitiveExpression, object data)
		{
			return null;
		}
		
		public override object Visit(BinaryOperatorExpression binaryOperatorExpression, object data)
		{
			// TODO : Operators 
			return binaryOperatorExpression.Left.AcceptVisitor(this, data);
		}
		
		public override object Visit(ParenthesizedExpression parenthesizedExpression, object data)
		{
			if (parenthesizedExpression == null) {
				return null;
			}
			return parenthesizedExpression.Expression.AcceptVisitor(this, data);
		}
		
		public override object Visit(InvocationExpression invocationExpression, object data)
		{
			if (invocationExpression.TargetObject is FieldReferenceExpression) {
				FieldReferenceExpression field = (FieldReferenceExpression)invocationExpression.TargetObject;
				TypeVisitor tv = new TypeVisitor (resolver);
				IReturnType type = field.TargetObject.AcceptVisitor(tv, data) as IReturnType;
				ArrayList methods = resolver.SearchMethod(type, field.FieldName);
				resolver.ShowStatic = false;
				if (methods.Count <= 0) {
					return null;
				}
				// TODO: Find the right method
				return methods[0];
			} else if (invocationExpression.TargetObject is IdentifierExpression) {
				string id = ((IdentifierExpression)invocationExpression.TargetObject).Identifier;
				if (resolver.CallingClass == null) {
					return null;
				}
				IReturnType type = new ReturnType(resolver.CallingClass.FullyQualifiedName);
				ArrayList methods = resolver.SearchMethod(type, id);
				resolver.ShowStatic = false;
				if (methods.Count <= 0) {
					return null;
				}
				// TODO: Find the right method
				return methods[0];
			}
			// invocationExpression is delegate call
			IReturnType t = invocationExpression.AcceptChildren(this, data) as IReturnType;
			if (t == null) {
				return null;
			}
			IClass c = resolver.SearchType(t.FullyQualifiedName, resolver.CompilationUnit);
			if (c.ClassType == ClassType.Delegate) {
				ArrayList methods = resolver.SearchMethod(t, "invoke");
				if (methods.Count <= 0) {
					return null;
				}
				return methods[0];
			}
			return null;
		}
		
		public override object Visit(FieldReferenceExpression fieldReferenceExpression, object data)
		{
			if (fieldReferenceExpression == null) {
				return null;
			}
			// int. generates a FieldreferenceExpression with TargetObject TypeReferenceExpression and no FieldName
			if (fieldReferenceExpression.FieldName == null || fieldReferenceExpression.FieldName == "") {
				if (fieldReferenceExpression.TargetObject is TypeReferenceExpression) {
//					resolver.ShowStatic = true;
//					return new ReturnType(((TypeReferenceExpression)fieldReferenceExpression.TargetObject).TypeReference);
					return null;
				}
			}
			TypeVisitor tv = new TypeVisitor (resolver);
			IReturnType returnType = fieldReferenceExpression.TargetObject.AcceptVisitor(tv, data) as IReturnType;
			if (returnType != null) {
				string name = resolver.SearchNamespace(returnType.FullyQualifiedName, resolver.CompilationUnit);
				if (name != null) {
					string n = resolver.SearchNamespace(string.Concat(name, ".", fieldReferenceExpression.FieldName), null);
					if (n != null) {
						return null;
					}
					IClass c = resolver.SearchType(string.Concat(name, ".", fieldReferenceExpression.FieldName), resolver.CompilationUnit);
					if (c != null) {
						resolver.ShowStatic = true;
						return c;
					}
					return null;
				}
				
				return resolver.SearchClassMember (returnType, fieldReferenceExpression.FieldName, true);
			}
			return null;
		}
		
		public override object Visit(PointerReferenceExpression pointerReferenceExpression, object data)
		{
			ReturnType type = pointerReferenceExpression.Expression.AcceptVisitor(this, data) as ReturnType;
			if (type == null) {
				return null;
			}
			type = type.Clone();
			--type.PointerNestingLevel;
			if (type.PointerNestingLevel != 0) {
				return null;
			}
			return resolver.SearchClassMember (type, pointerReferenceExpression.Identifier, true);
		}
		
		public override object Visit(IdentifierExpression identifierExpression, object data)
		{
			//Console.WriteLine("visiting IdentifierExpression");
			if (identifierExpression == null) {
				return null;
			}
			string name = resolver.SearchNamespace(identifierExpression.Identifier, resolver.CompilationUnit);
			if (name != null) {
				return new ReturnType(name);
			}
			IClass c = resolver.SearchType(identifierExpression.Identifier, resolver.CompilationUnit);
			if (c != null) {
				resolver.ShowStatic = true;
				return new ReturnType(c.FullyQualifiedName);
			}
			return resolver.IdentifierLookup (identifierExpression.Identifier);
		}
		
		public override object Visit(TypeReferenceExpression typeReferenceExpression, object data)
		{
			return resolver.SearchType (typeReferenceExpression.TypeReference.Type, resolver.CompilationUnit);
		}
		
		public override object Visit(UnaryOperatorExpression unaryOperatorExpression, object data)
		{
			return null;
		}
		
		public override object Visit(AssignmentExpression assignmentExpression, object data)
		{
			return assignmentExpression.Left.AcceptVisitor(this, data);
		}
		
		public override object Visit(SizeOfExpression sizeOfExpression, object data)
		{
			return null;
		}
		
		public override object Visit(TypeOfExpression typeOfExpression, object data)
		{
			return null;
		}
		
		public override object Visit(CheckedExpression checkedExpression, object data)
		{
			return checkedExpression.Expression.AcceptVisitor(this, data);
		}
		
		public override object Visit(UncheckedExpression uncheckedExpression, object data)
		{
			return uncheckedExpression.Expression.AcceptVisitor(this, data);
		}
		
		public override object Visit(CastExpression castExpression, object data)
		{
			return null;
		}
		
		public override object Visit(StackAllocExpression stackAllocExpression, object data)
		{
			return null;
		}
		
		public override object Visit(IndexerExpression indexerExpression, object data)
		{
			IReturnType type = (IReturnType)indexerExpression.TargetObject.AcceptVisitor(this, data);
			if (type == null) {
				return null;
			}
			if (type.ArrayDimensions == null || type.ArrayDimensions.Length == 0) {
				if (indexerExpression.TargetObject is ThisReferenceExpression) {
					return null;
				}
				ArrayList indexer = resolver.SearchIndexer(type);
				if (indexer.Count == 0) {
					return null;
				}
				// TODO: get the right indexer
				return indexer[0];
			}
			
			return null;
		}
		
		public override object Visit(ThisReferenceExpression thisReferenceExpression, object data)
		{
			return null;
		}
		
		public override object Visit(BaseReferenceExpression baseReferenceExpression, object data)
		{
			if (resolver.CallingClass == null) {
				return null;
			}
			IClass baseClass = resolver.BaseClass(resolver.CallingClass);
			if (baseClass == null) {
				return null;
			}
			return baseClass;
		}
		
		public override object Visit(ObjectCreateExpression objectCreateExpression, object data)
		{
			return null;
		}
		
		public override object Visit(ArrayCreateExpression arrayCreateExpression, object data)
		{
			return null;
		}
		
		public override object Visit(DirectionExpression directionExpression, object data)
		{
			// no calls allowed !!!
			return null;
		}
		
		public override object Visit(ArrayInitializerExpression arrayInitializerExpression, object data)
		{
			// no calls allowed !!!
			return null;
		}
	}
}
