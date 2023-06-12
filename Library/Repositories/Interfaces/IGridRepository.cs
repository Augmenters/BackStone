using System;
using Library.Models;

namespace Library.Repositories.Interfaces
{
	public interface IGridRepository
	{
        string GenerateHash(Coordinate coordinate);
        Coordinate DecodeHash(string hash);
        bool VerifyHashContainsCoordinate(string hash, Coordinate coordinate);
        IEnumerable<GridBox> GenerateGrid(Coordinate coordinate);
    }
}

