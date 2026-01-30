namespace Vacuum.Data.Enums;

public enum MarketOrderType
{
    Buy,
    Sell
}

public enum MarketOrderStatus
{
    Active,
    Partial,
    Filled,
    Cancelled,
    Expired
}

public enum MarketAccessLevel
{
    Full,
    Restricted,
    Denied
}

public enum MarketSortOption
{
    Name,
    Price,
    Volume,
    Change
}

public enum ItemCategory
{
    Ore,
    Mineral,
    Module,
    Ship,
    Ammunition,
    Component,
    Blueprint,
    Consumable,
    Misc
}

public enum TradeRouteStatus
{
    Planning,
    InTransit,
    Delivered,
    Failed
}

public enum HaulingContractStatus
{
    Open,
    Accepted,
    InProgress,
    Completed,
    Expired,
    Failed
}
