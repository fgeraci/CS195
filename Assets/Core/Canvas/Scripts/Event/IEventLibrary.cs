using UnityEngine;
using System.Collections;

public interface IEventLibrary
{
    EventDescriptor GetDescriptor(string name);
}
