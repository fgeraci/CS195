using TreeSharpPlus;

[LibraryIndex(5)]
public class Merge : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter participant1, SmartCharacter participant2)
    {
        return new LeafTrace(this.GetType().Name);
    }

    [StateRequired(0, StateName.RoleActor, StateName.RoleGuard1)]
    [StateRequired(1, StateName.RoleActor, StateName.RoleTeller)]
    [StateRequired(2, StateName.RoleChair)]
    [Merge(typeof(MergeTarget))]
    public Merge(SmartCharacter c1, SmartCharacter c2, SmartChair chair)
        : base(c1, c2) { }
}

[LibraryIndex(5)]
public class MergeTarget : GenericEvent<SmartCharacter, SmartCharacter>
{
    protected override Node Root(Token token, SmartCharacter participant1, SmartCharacter participant2)
    {
        return new LeafTrace(this.GetType().Name);
    }

    [StateRequired(0, StateName.RoleActor, StateName.RoleGuard2)]
    [StateRequired(1, StateName.RoleActor, ~StateName.RoleTeller)]
    [MergeAt(typeof(Merge))]
    public MergeTarget(SmartCharacter c1, SmartCharacter c2)
        : base(c1, c2) { }

}