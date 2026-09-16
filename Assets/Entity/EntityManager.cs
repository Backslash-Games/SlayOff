using System;
using System.Collections.Generic;
using UnityEngine;
using static EntityData;

public class EntityManager : MonoBehaviour
{
    private bool _initialized = false;
    public List<TeamCollection> teams;

    [System.Serializable]
    public struct TeamCollection
    {
        public Team team;
        public List<EntityData> members;
    }

    #region Singleton
    private static EntityManager _instance = null;
    public static EntityManager Instance { get { return _instance; } }

    private void CreateSingleton()
    {
        // -> Pulled from Out on the Red Sea
        // Checks if the instance of object is first of its type
        // If object is not unique, destroy current instance
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        // Declares this script as current
        else
        {
            _instance = this;
        }
    }
    #endregion
    #region Unity Method
    private void Awake()
    {
        CreateSingleton();
        CreateTeamList();
    }
    #endregion

    #region List Management
    /// <summary>
    ///     Creates the team list
    /// </summary>
    private void CreateTeamList()
    {
        // Check if the list is initialized
        if (_initialized) return;
        // Set flag
        _initialized = true;

        // Create list of lists
        teams = new List<TeamCollection>();

        // Creates a struct array based on team length
        Array array = Enum.GetValues(typeof(Team));
        for(int i = 0; i < array.Length; i++)
        {
            TeamCollection collection = new TeamCollection();
            // Construct
            collection.team = (Team)i;
            collection.members = new List<EntityData>();
            // Assign
            teams.Add(collection);
        }
    }

    /// <summary>
    ///     Adds enemy to active with proper handling
    /// </summary>
    /// <param name="enemy">New enemy</param>
    public void AddToActive(EntityData entity)
    {
        CreateTeamList();
        int index = (int)entity.team;
        // Check if team is in list range
        if (!TeamInRange(index)) return;
        // Try to add entity to list
        if (!teams[index].members.Contains(entity)) teams[index].members.Add(entity);
    }
    /// <summary>
    ///     Removes enemy from active with proper handling
    /// </summary>
    /// <param name="enemy">Existing enemy</param>
    public void RemoveFromActive(EntityData entity)
    {
        CreateTeamList();
        int index = (int)entity.team;
        // Check if team is in list range
        if (!TeamInRange(index)) return;
        // Try to remove entity from list
        teams[index].members.Remove(entity);
    }
    #endregion
    #region List Checking
    /// <summary>
    ///     Checks if the team value is in entitiy range. Ignores entities with no team
    /// </summary>
    /// <param name="team">Input team</param>
    /// <returns>True if value can be applied to list</returns>
    private bool TeamInRange(Team team) { return TeamInRange((int)team); }

    /// <summary>
    ///     Checks if the team value is in entity range. Ignores entities with no team
    /// </summary>
    /// <param name="value">Input value</param>
    /// <returns>True if value can be applied to list</returns>
    private bool TeamInRange(int value) { return value > 0 && value < teams.Count; }
    #endregion
    #region List Pulling
    /// <summary>
    ///     Pulls all entieis from a team
    /// </summary>
    /// <param name="team">Target team</param>
    /// <returns>List of team</returns>
    public List<EntityData> GetEntitiesOnTeam(Team team)
    {
        int index = (int)team;
        // Checks if the team is in range
        if (!TeamInRange(index)) return new List<EntityData>();
        return new List<EntityData>(teams[index].members);
    }

    /// <summary>
    ///     Pulls all entities from all opposing teams
    /// </summary>
    /// <param name="excludedTeam">Excluded team</param>
    /// <returns>List of every other entity</returns>
    public List<EntityData> GetEntitiesOnOtherTeams(Team excludedTeam)
    {
        return GetEntitiesOnOtherTeams(new Team[] { excludedTeam });
    }/// <summary>
     ///     Pulls all entities from all opposing teams
     /// </summary>
     /// <param name="excludedTeams">Excluded teams</param>
     /// <returns>List of every other entity</returns>
    public List<EntityData> GetEntitiesOnOtherTeams(Team[] excludedTeams)
    {
        // Establish a list
        List<EntityData> c_List = new List<EntityData>();
        List<Team> c_Teams = new List<Team>(excludedTeams);

        // Establish full list
        for (int i = 1; i < teams.Count; i++)
        {
            // Check if we are excluded
            if (c_Teams.Contains((Team)i)) continue;
            // Add all entities from current team
            c_List.AddRange(GetEntitiesOnTeam((Team)i));
        }
        return c_List;
    }
    #endregion
}
