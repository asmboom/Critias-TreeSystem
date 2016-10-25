// Copyright Ioan-Bogdan Lazu. All Rights Reserved.

/**
 * Row and column for grid cell
 */
[System.Serializable]
public struct RowCol
{
    public int m_Row;
    public int m_Col;

    public RowCol(int row, int col)
    {
        m_Row = row;
        m_Col = col;
    }

    public void SetCellData(int row, int col)
    {
        m_Row = row;
        m_Col = col;
    }

    public int GetCol()
    {
        return m_Col;
    }

    public int GetRow()
    {
        return m_Row;
    }

    /**
     * Get index in a linear array from these two values
     */
    public int GetIndex1D(int maxColumns)
    {
        return m_Row * maxColumns + m_Col;
    }

    public static int GetIndex1D(int row, int col, int maxColumns)
    {
        return row * maxColumns + col;
    }

    public bool IsSameCell(ref RowCol other)
    {
        if (m_Row != other.m_Row || m_Col != other.m_Col)
            return false;

        return true;
    }

    public bool IsSameCell(RowCol other)
    {
        if (m_Row != other.m_Row || m_Col != other.m_Col)
            return false;

        return true;
    }
}
