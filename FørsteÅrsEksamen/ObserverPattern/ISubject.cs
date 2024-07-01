﻿namespace DoctorsDungeon.ObserverPattern;

// Oscar
public interface ISubject
{
    public void Attach(IObserver observer);

    public void Detach(IObserver observer);

    public void Notify();
}