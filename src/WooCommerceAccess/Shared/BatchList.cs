using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WooCommerceAccess.Shared
{
	public class BatchList< T > : IEnumerable< IEnumerable< T > >
	{
		private int _batchOffset;
		private readonly int _batchSize;
		private readonly IEnumerable< T > _list;

		public BatchList( IEnumerable< T > list, int batchSize )
		{
			_batchOffset = 0;
			this._batchSize = batchSize;
			this._list = list;
		}

		public IEnumerator< IEnumerable< T > > GetEnumerator()
		{
			IEnumerable< T > batch;
			while( ( batch = GetNextBatch().ToList() ).Any() )
			{
				yield return batch;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable< T > GetNextBatch()
		{
			var batch = _list.Skip( _batchOffset ).Take( _batchSize ).ToList();
			_batchOffset += batch.Count();
			return batch;
		}
	}
}
