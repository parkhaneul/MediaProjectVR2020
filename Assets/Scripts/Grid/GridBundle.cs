﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBundle
{
    public Placable owner;
    public List<Grid> grids;

    public GridBundle(Grid source, Placable owner, List<Grid> searchTargets)
    {
        HashSet<Grid> touchingGrids = new HashSet<Grid>();
        List<Grid> copy = new List<Grid>(searchTargets);
        SearchTouchingGrids(source, owner, copy, touchingGrids);

        this.owner = owner;
        grids = new List<Grid>();
        grids.Add(source);
        searchTargets.Remove(source);
        foreach(var grid in touchingGrids)
        {
            searchTargets.Remove(grid);
            grids.Add(grid);
        }
    }

    private void SearchTouchingGrids(Grid target, Placable ownerOfTarget, List<Grid> targets, HashSet<Grid> collections)
    {
        targets.Remove(target);
        List<Grid> grids = GridManager.GetTouchingGrids(target, targets);
        int prevCount = collections.Count;
        foreach(var grid in grids)
        {
            if(ownerOfTarget == grid.owner)
                collections.Add(grid);
        }
        if(grids.Count == prevCount)
        {
            return;
        }
        foreach(var grid in grids)
        {
            SearchTouchingGrids(grid, ownerOfTarget, targets, collections);
        }
    } 

    public List<Grid> GetAdjacentGrids()
    {
        HashSet<Grid> adjacents = new HashSet<Grid>();
        foreach(var grid in grids)
        {
            foreach(var neighbor in grid.adjacentGrids)
            {
                adjacents.Add(neighbor);
            }
        }
        foreach(var grid in grids)
        {
            adjacents.Remove(grid);
        }
        return new List<Grid>(adjacents);
    }

    public List<Grid> GetVacantAdjacentGrids()
    {
        HashSet<Grid> adjacents = new HashSet<Grid>();
        foreach(var grid in grids)
        {
            foreach(var neighbor in grid.adjacentGrids)
            {
                if(!neighbor.isOccupied)
                    adjacents.Add(neighbor);
            }
        }
        foreach(var grid in grids)
        {
            adjacents.Remove(grid);
        }
        return new List<Grid>(adjacents);
    }

    public Grid GetGridFromDirection(Vector3 dir)
    {
        if(grids.Count != 1)
        {
            Debug.LogError("Error");
            return null;
        }

        Grid gridInDirection = grids[0].adjacentGrids[0];
        float dot = Vector3.Dot((gridInDirection.gridCenter - grids[0].gridCenter).normalized, dir);
        foreach (var grid in grids[0].adjacentGrids)
        {
            float newDot = Vector3.Dot((grid.gridCenter - grids[0].gridCenter).normalized, dir);
            if (newDot > dot)
            {
                dot = newDot;
                gridInDirection = grid;
            }
        }
        return gridInDirection;
    }

    public void MoveToNewGrid(Grid grid)
    {
        if(grids.Count != 1)
        {
            Debug.LogError("Error");
            return;
        }

        grids[0].Unoccupy();
        grids.Clear();
        grids.Add(grid);
        grid.Occupy(owner);
    }
}
