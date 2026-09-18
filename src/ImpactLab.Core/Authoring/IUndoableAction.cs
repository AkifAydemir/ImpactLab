namespace ImpactLab.Core.Authoring;

public interface IUndoableAction
{
    string Name { get; }
    void Execute();
    void Undo();
    bool TryMerge(IUndoableAction newer) => false;
}
