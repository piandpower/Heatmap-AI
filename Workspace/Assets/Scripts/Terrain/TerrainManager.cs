﻿using UnityEngine;
using System.Collections;

public class TerrainManager : MonoBehaviour 
{
	public enum AnalysisType{ HardCodedValues, Height, ViewDistance, ShootingDistance };

	public AnalysisType TypeOfAnalysisToUse = AnalysisType.Height;

	// used in terrain analysis
	public float maxTerrainHeat; // must be > 0

	// used to render the colours properly on the tiles
	public bool RenderHeatOnTiles = true;
	public bool PutPlayerInfluenceInTiles = false; // allows for debugging and data collection -- may interfere with AI
	public float MaxHeat;
	public float PlayerCenterInfluence; // heat at center of influence
	public int PlayerInfluenceRadius; // 0 means the player only has influence on their square, 1 means 1 square away

	private HSBColor P1Color = HSBColor.FromColor(Color.red);
	private HSBColor P2Color = HSBColor.FromColor(Color.blue);

	public Transform[,] RawBoard;
	public TileProperties[,] AnalyzedBoard;
	public PlayerInfluenceMap PlayerInfluence; // no board influence, just the players

	// the bonuses must be >= 1
	private float HighGroundInfluenceBonus = 2f;

	private AbstractTerrainAnalyzer analyzer;
	private PlayerManager players;

	void Start()
	{
		int rows = transform.childCount;
		int cols = transform.GetChild (0).childCount;
		RawBoard = new Transform[rows, cols];
		UpdateRawBoardValues ();

		if( TypeOfAnalysisToUse != AnalysisType.HardCodedValues)
			AnalyzeTerrain ();

		PlayerInfluence = new PlayerInfluenceMap(rows, cols, PlayerCenterInfluence, PlayerInfluenceRadius, HighGroundInfluenceBonus, RawBoard);
		players = GetComponent<PlayerManager> ();
	}
	
	void Update()
	{
		PlayerInfluence.UpdatePlayerInfluenceMap(players.RedPlayer, players.BluePlayer);
		if( RenderHeatOnTiles )
		{
			RenderInfluenceMap();
		}
	}

	private void AnalyzeTerrain()
	{
		if (TypeOfAnalysisToUse == AnalysisType.Height)
			analyzer = gameObject.AddComponent<HeightAnalyzer>();
		else if (TypeOfAnalysisToUse == AnalysisType.ViewDistance )
		{
			analyzer = gameObject.AddComponent<ViewDistanceAnalyzer>();
			GetComponent<ViewDistanceAnalyzer>().UnitViewRadius = 99999;
		}
		else if (TypeOfAnalysisToUse == AnalysisType.ShootingDistance )
		{
			analyzer = gameObject.AddComponent<ShootingDistanceAnalyzer>();
			GetComponent<ShootingDistanceAnalyzer>().UnitViewRadius = PlayerInfluenceRadius;
		}
		analyzer.level = RawBoard;
		analyzer.maxTerrainHeat = maxTerrainHeat; // must be > 0
		analyzer.AnalyzeTerrain ();
		RawBoard = analyzer.level;
	}

	private void UpdateRawBoardValues()
	{
		for(int i = 0; i < transform.childCount; i++)
		{
			Transform row = transform.GetChild(i);
			for( int j = 0; j < row.childCount; j++)
			{
				RawBoard[i,j] = row.GetChild(j);
			}
		}
	}

	// Renders the heat without player influence
	private void RenderInfluenceMap()
	{
		for(int i = 0; i < transform.childCount; i++)
		{
			Transform row = transform.GetChild(i);
			for( int j = 0; j < row.childCount; j++)
			{
				Transform tile = row.GetChild(j);

				Renderer rend = tile.GetComponent<Renderer>();
				float heat = PlayerInfluence.InfluenceMap[i,j];

				if(PutPlayerInfluenceInTiles)
					tile.GetComponent<TileProperties>().BaseHeat = heat;

				if(heat >= 0)
				{
					P1Color.s = (heat/MaxHeat);
					rend.material.color = P1Color.ToColor();
				}
				else if(heat < 0)
				{
					P2Color.s = Mathf.Abs(heat/MaxHeat);
					rend.material.color = P2Color.ToColor();
				}
			}
		}
	}
}
