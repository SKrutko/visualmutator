﻿namespace VisualMutator.OperatorsObject.Operators.Methods
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Extensibility;
    using log4net;
    using Microsoft.Cci;
    using Microsoft.Cci.MutableCodeModel;
    using UsefulTools.ExtensionMethods;

    public class OMR_OverloadingMethodContentsChange : IMutationOperator
    {
        protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public OperatorInfo Info
        {
            get
            {
                return new OperatorInfo("OMR", "Overloading method contents change", "");
            }
        }

        public static string GetMethodSignatureString(IMethodDefinition method)
        {
            return null;
            //return method.Name +" "+method.Parameters.Select(p => p.ContainingSignature)Aggregate((p1, p2) => p1.Type.)
        }

        public class OMRVisitor : OperatorCodeVisitor
        {
            private List<IMethodDefinition> FindCandidateMethods(IMethodDefinition method)
            {
                var methodsInThisType = method.ContainingTypeDefinition.Methods
                    .Where(m => m != method && m.Name.Value == method.Name.Value);

                return methodsInThisType.ToList();
            }

            public override void Visit(IMethodBody body)
            {
                var method = body.MethodDefinition;
                _log.Info("Visiting IMethodBody of: " + method);

                var methods = FindCandidateMethods(method);
                var compatibileMethods = methods.Where(m => m.Parameters
                    .All(p => method.Parameters.Any(p2 => p2.Type == p.Type))).ToList();

                if (compatibileMethods.Count != 0)
                {
                    MarkMutationTarget(body, compatibileMethods.First().ToString().InList());
                }
            }
        }

        public class OMRRewriter : OperatorCodeRewriter
        {
            public override IMethodBody Rewrite(IMethodBody body)
            {
                var method = body.MethodDefinition;
                _log.Info("Rewriting IMethodBody of: " + method + " Pass: " + MutationTarget.PassInfo);

                var newBody = new SourceMethodBody(Host)
                {
                    MethodDefinition = method,
                    LocalsAreZeroed = true
                };
                var block = new BlockStatement();
                newBody.Block = block;

                var replacement = method.ContainingTypeDefinition.Methods.Single(m => m.ToString() == MutationTarget.PassInfo);
                var methodCall = new MethodCall
                {
                    MethodToCall = replacement,
                    Type = replacement.Type,
                    ThisArgument = new ThisReference() { Type = method.ContainingTypeDefinition }
                };
                foreach (var param in replacement.Parameters)
                {
                    methodCall.Arguments.Add(new BoundExpression()
                    {
                        Definition = method.Parameters
                                .First(p =>
                                    ((INamedTypeReference)p.Type).Name.Value ==
                                    ((INamedTypeReference)param.Type).Name.Value)
                    });
                    //  methodCall.Arguments.Add(method.Parameters.First(p => new ));
                }

                if (replacement.Type == Host.PlatformType.SystemVoid)
                {
                    block.Statements.Add(new ExpressionStatement
                    {
                        Expression = methodCall
                    });
                    block.Statements.Add(new ReturnStatement());
                }
                else
                {
                    block.Statements.Add(new ReturnStatement
                    {
                        Expression = methodCall
                    });
                }

                return newBody;
            }
        }

        private OMRVisitor visitor = new OMRVisitor();

        public IOperatorCodeVisitor CreateVisitor()
        {
            return visitor;
        }

        public IOperatorCodeRewriter CreateRewriter()
        {
            return new OMRRewriter();
        }
    }
}