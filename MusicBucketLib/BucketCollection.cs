using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace MusicBucketLib
{
    /// <summary>
    /// A list of Buckets, used to feed <see cref="BucketListDisplay"/>.
    /// </summary>
    public class BucketCollection: IEnumerable, IList<Bucket>,INotifyCollectionChanged
    {
        List<Bucket> _internalList;
        public event NotifyCollectionChangedEventHandler CollectionChanged;


        public List<Bucket> BucketCollectionContent
        {
            get 
            {
                return _internalList;
            }
            set
            {
                _internalList = value;
            }
        }

        public BucketCollection()
        {
            _internalList = new List<Bucket>();
        }

        public IEnumerator GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }


        public int IndexOf(Bucket item)
        {
            return _internalList.IndexOf(item);
        }

        public void Insert(int index, Bucket item)
        {
            _internalList.Insert(index, item);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(new NotifyCollectionChangedAction()));
            }
        }

        public void RemoveAt(int index)
        {
            _internalList.RemoveAt(index);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(new NotifyCollectionChangedAction()));
            }
        }

        public Bucket this[int index]
        {
            get
            {
                return _internalList[index];

            }
            set
            {
                _internalList[index] = value;
                if (CollectionChanged != null)
                {
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(new NotifyCollectionChangedAction()));
                }
            }
        }

        public void Add(Bucket item)
        {
            _internalList.Add(item);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(new NotifyCollectionChangedAction()));
            }
        }

        public void Clear()
        {
            _internalList.Clear();
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(new NotifyCollectionChangedAction()));
            }
        }

        public bool Contains(Bucket item)
        {
            return _internalList.Contains(item);
        }

        public void CopyTo(Bucket[] array, int arrayIndex)
        {
            _internalList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _internalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Bucket item)
        {
            bool res;
            res = _internalList.Remove(item);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(new NotifyCollectionChangedAction()));
            }
            return res;
        }

        IEnumerator<Bucket> IEnumerable<Bucket>.GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }


    }
}
