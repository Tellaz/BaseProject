using BaseProject.DAO.Models.Filters;
using System.Linq.Expressions;

namespace BaseProject.DAO.Models.Others
{
    public class DTParam
    {
        public int Page { get; set; }

        public int RowsPerPage { get; set; }

        public string Search { get; set; }

        public string Order { get; set; }

        public string OrderBy { get; set; }

        public DefaultFM Filters { get; set; }

        public int InitialPosition()
        {
            return Page * RowsPerPage;
        }

        public int ItensPerPage()
        {
            return RowsPerPage;
        }

        public string SearchValue()
        {
            return Search;
        }

        public bool IsAscendingSort()
        {
            return Order == null || Order == "asc";
        }

        public string SortedColumnName()
        {
            if (Order == null) return "Nome";

            return string.IsNullOrEmpty(OrderBy) ? "Nome" : OrderBy;
        }

    }

    public class DTParam<TEntity>
    {
        public int Page { get; set; }

        public int RowsPerPage { get; set; }

        public string Search { get; set; }

        public string Order { get; set; }

        public string OrderBy { get; set; }

        public TEntity Filters { get; set; }

        public int InitialPosition()
		{
            return Page * RowsPerPage;
        }

        public int ItensPerPage()
        {
            return RowsPerPage;
        }

        public string SearchValue()
        {
            return Search;
        }

        public bool IsAscendingSort()
		{
            return Order == null || Order == "asc";
		}

        public string SortedColumnName()
		{
            if (Order == null) return "Nome";

            return string.IsNullOrEmpty(OrderBy) ? "Nome" : OrderBy;
		}

    }

    public class DTResult<T>
    {
        public T[] Itens { get; set; }
        public int Total { get; set; }
    }

    public class KeySelectorGenerator<TEntity>
    {
        public KeySelectorGenerator(string sortedColumnName)
        {
            StringKeySelectors = new Dictionary<string, Expression<Func<TEntity, string>>>();
            DoubleKeySelectors = new Dictionary<string, Expression<Func<TEntity, double>>>();
            IntKeySelectors = new Dictionary<string, Expression<Func<TEntity, int>>>();
            FloatKeySelectors = new Dictionary<string, Expression<Func<TEntity, float>>>();
            DatetimeKeySelector = new Dictionary<string, Expression<Func<TEntity, DateTime>>>();
            BoolKeySelectors = new Dictionary<string, Expression<Func<TEntity, bool>>>();
            SelectorNameAndType = new Dictionary<string, string>();
            SortedColumnName = sortedColumnName;
        }

        private Dictionary<string, Expression<Func<TEntity, string>>> StringKeySelectors { get; set; }
        private Dictionary<string, Expression<Func<TEntity, double>>> DoubleKeySelectors { get; set; }
        private Dictionary<string, Expression<Func<TEntity, int>>> IntKeySelectors { get; set; }
        private Dictionary<string, Expression<Func<TEntity, float>>> FloatKeySelectors { get; set; }
        private Dictionary<string, Expression<Func<TEntity, DateTime>>> DatetimeKeySelector { get; set; }
        private Dictionary<string, Expression<Func<TEntity, bool>>> BoolKeySelectors { get; set; }
        private Dictionary<string, string> SelectorNameAndType { get; set; }
        private string SortedColumnName { get; set; }

        public void AddKeySelector(Expression<Func<TEntity, string>> keySelector, string keyName)
        {
            StringKeySelectors.Add(keyName, keySelector);
            SelectorNameAndType[keyName] = "String";
        }

        public void AddKeySelector(Expression<Func<TEntity, DateTime>> keySelector, string keyName)
        {
            DatetimeKeySelector.Add(keyName, keySelector);
            SelectorNameAndType[keyName] = "DateTime";
        }

        public void AddKeySelector(Expression<Func<TEntity, int>> keySelector, string keyName)
        {
            IntKeySelectors.Add(keyName, keySelector);
            SelectorNameAndType[keyName] = "Integer";
        }

        public void AddKeySelector(Expression<Func<TEntity, float>> keySelector, string keyName)
        {
            FloatKeySelectors.Add(keyName, keySelector);
            SelectorNameAndType[keyName] = "Float";
        }

        public void AddKeySelector(Expression<Func<TEntity, double>> keySelector, string keyName)
        {
            DoubleKeySelectors.Add(keyName, keySelector);
            SelectorNameAndType[keyName] = "Double";
        }

        public void AddKeySelector(Expression<Func<TEntity, bool>> keySelector, string keyName)
        {
            BoolKeySelectors.Add(keyName, keySelector);
            SelectorNameAndType[keyName] = "Boolean";
        }

        public IQueryable<TEntity> Sort(IQueryable<TEntity> query, bool ascendingSort)
        {
            string keyName = this.SortedColumnName;

            if (!SelectorNameAndType.ContainsKey(keyName))
            {
                keyName = SelectorNameAndType.First().Key;
            }

            string type = SelectorNameAndType[keyName];

            switch (type)
            {
                case "String":
                    query = this.GetOrderSequence(query, ascendingSort, StringKeySelectors[keyName]);
                    break;
                case "Double":
                    query = this.GetOrderSequence(query, ascendingSort, DoubleKeySelectors[keyName]);
                    break;
                case "DateTime":
                    query = this.GetOrderSequence(query, ascendingSort, DatetimeKeySelector[keyName]);
                    break;
                case "Integer":
                    query = this.GetOrderSequence(query, ascendingSort, IntKeySelectors[keyName]);
                    break;
                case "Float":
                    query = this.GetOrderSequence(query, ascendingSort, FloatKeySelectors[keyName]);
                    break;
                case "Boolean":
                    query = this.GetOrderSequence(query, ascendingSort, BoolKeySelectors[keyName]);
                    break;
                default:
                    break;
            }

            return query;
        }

        private IQueryable<TEntity> GetOrderSequence<TValue>(IQueryable<TEntity> query, bool ascendingSort, Expression<Func<TEntity, TValue>> selector)
        {
            if (ascendingSort)
            {
                return query.OrderBy(selector);
            }
            else
            {
                return query.OrderByDescending(selector);
            }
        }
        
    }
}
