using System;

namespace Antlr.Runtime.Tree;

public class TreeFilter(ITreeNodeStream input, RecognizerSharedState state) : TreeParser(input, state)
{
	protected ITokenStream originalTokenStream = input.TokenStream;

	protected ITreeAdaptor originalAdaptor = input.TreeAdaptor;

	public TreeFilter(ITreeNodeStream input)
		: this(input, new RecognizerSharedState())
	{
	}

	public virtual void ApplyOnce(object t, Action whichRule)
	{
		if (t == null)
		{
			return;
		}
		try
		{
			state = new RecognizerSharedState();
			input = new CommonTreeNodeStream(originalAdaptor, t);
			((CommonTreeNodeStream)input).TokenStream = originalTokenStream;
			BacktrackingLevel = 1;
			whichRule();
			BacktrackingLevel = 0;
		}
		catch (RecognitionException)
		{
		}
	}

	public virtual void Downup(object t)
	{
		TreeVisitor treeVisitor = new TreeVisitor(new CommonTreeAdaptor());
		Func<object, object> preAction = (object o) =>
		{
			ApplyOnce(o, Topdown);
			return o;
		};
		Func<object, object> postAction = (object o) =>
		{
			ApplyOnce(o, Bottomup);
			return o;
		};
		treeVisitor.Visit(t, preAction, postAction);
	}

	protected virtual void Topdown()
	{
	}

	protected virtual void Bottomup()
	{
	}
}
