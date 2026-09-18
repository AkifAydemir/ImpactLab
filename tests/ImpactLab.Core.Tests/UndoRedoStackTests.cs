using ImpactLab.Core.Authoring;
using Xunit;

namespace ImpactLab.Core.Tests;

public sealed class UndoRedoStackTests
{
    [Fact]
    public void UndoAndRedoRestoreValues()
    {
        var v = 1;
        var stack = new UndoRedoStack();
        stack.Execute(new PropertyChangeAction<int>("set", "v", 1, 2, x => v = x));
        Assert.Equal(2, v);
        stack.Undo();
        Assert.Equal(1, v);
        stack.Redo();
        Assert.Equal(2, v);
    }
}
