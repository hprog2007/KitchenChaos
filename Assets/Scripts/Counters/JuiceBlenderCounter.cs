using System;
using UnityEngine;

public class JuiceBlenderCounter : BaseCounter, IHasProgress
{
    public event EventHandler<IHasProgress.OnProgressChangedEventArgs> OnProgressChanged;
}
