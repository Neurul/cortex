using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.neurul.Brain.Domain.Model.Neurons
{
    // TODO: Adapt and Update
    //public class ItemRelationService
    //{
    //    public ItemRelationService(IItemRepository itemRepository)
    //    {
    //        this.itemRepository = itemRepository;
    //    }

    //    private IItemRepository itemRepository;

    //    public void Save(Item item)
    //    {
    //        ReadOnlyCollection<ParentInfo> parents = item.Parents;
    //        // update parent infos
    //        foreach(ParentInfo pi in parents)
    //            // if predecessor is empty and item is not first child
    //            if (pi.PredecessorId == ItemId.Empty && this.itemRepository.GetFirstChildId(pi.ParentId) != item.Id)
    //                item.UpdatePredecessor(pi.ParentId, this.itemRepository.GetLastChildId(pi.ParentId));

    //        this.itemRepository.Save(item);
    //    }

    //    public void Insert(ItemId parentId, ItemId successorId, Item item)
    //    {
    //        if (parentId.Equals(ItemId.Empty))
    //            throw new ArgumentNullException(nameof(parentId));
    //        if (successorId.Equals(ItemId.Empty))
    //            throw new ArgumentNullException(nameof(successorId));
    //        if (item == null)
    //            throw new ArgumentNullException(nameof(item));

    //        Item successor = this.itemRepository.GetById(successorId);
    //        ItemId successorPreviousPredecessorId = ItemId.Empty;
    //        ParentInfo parentItem = ParentInfo.Empty;

    //        if (Item.TryGetParent(successor, parentId, out parentItem))
    //            successorPreviousPredecessorId = parentItem.PredecessorId;
    //        successor.UpdatePredecessor(parentId, item.Id);

    //        if (!item.HasParent(parentId))
    //            item.AddParent(parentId);

    //        item.UpdatePredecessor(parentId, successorPreviousPredecessorId);

    //        this.itemRepository.Save(successor);
    //        this.itemRepository.Save(item);
    //    }

    //    public void Move(ItemId parentId, Direction direction, Item item)
    //    {
    //        if (parentId.Equals(ItemId.Empty))
    //            throw new ArgumentNullException(nameof(parentId));
    //        if (item == null)
    //            throw new ArgumentNullException(nameof(item));

    //        Item parent = this.itemRepository.GetById(parentId);
    //        ParentInfo parentItem = ParentInfo.Empty;
    //        ItemId successorId = ItemId.Empty;

    //        switch (direction)
    //        {
    //            case Direction.Previous:
    //                // get current predecessor
    //                if (Item.TryGetParent(item, parentId, out parentItem))
    //                {
    //                    ItemId currentPredecessorId = parentItem.PredecessorId;

    //                    // if item is not yet first item under parent
    //                    if (currentPredecessorId != ItemId.Empty)
    //                    {
    //                        Item predecessor = this.itemRepository.GetById(currentPredecessorId);
    //                        if (Item.TryGetParent(predecessor, parentId, out parentItem))
    //                        {
    //                            // get successor of current item
    //                            successorId = this.itemRepository.GetNextSiblingId(parentId, item.Id);
    //                            // update predecessor of current item to predecessor of initial predecessor
    //                            item.UpdatePredecessor(parentId, parentItem.PredecessorId);
    //                            // update predecessor of initial predecessor to current item
    //                            predecessor.UpdatePredecessor(parentId, item.Id);
    //                            // if successor exists
    //                            if (successorId != ItemId.Empty)
    //                            {
    //                                // update predecessor of initial successor to initial predecessor
    //                                Item successor = this.itemRepository.GetById(successorId);
    //                                successor.UpdatePredecessor(parentId, predecessor.Id);
    //                                this.itemRepository.Save(successor);
    //                            }

    //                            this.itemRepository.Save(item);
    //                            this.itemRepository.Save(predecessor);
    //                        }
    //                    }
    //                }
    //                break;
    //            case Direction.Next:
    //                successorId = this.itemRepository.GetNextSiblingId(parentId, item.Id);

    //                // get current predecessor
    //                if (successorId != ItemId.Empty && Item.TryGetParent(item, parentId, out parentItem))
    //                {
    //                    ItemId currentPredecessorId = parentItem.PredecessorId;
    //                    Item successor = this.itemRepository.GetById(successorId);

    //                    // get successor of successor
    //                    ItemId successorSuccessorId = this.itemRepository.GetNextSiblingId(parentId, successorId);
    //                    if (successorSuccessorId != ItemId.Empty)
    //                    {
    //                        Item successorSuccessor = this.itemRepository.GetById(successorSuccessorId);
    //                        successorSuccessor.UpdatePredecessor(parentId, item.Id);
    //                        this.itemRepository.Save(successorSuccessor);
    //                    }

    //                    successor.UpdatePredecessor(parentId, currentPredecessorId);
    //                    item.UpdatePredecessor(parentId, successorId);

    //                    this.itemRepository.Save(successor);
    //                    this.itemRepository.Save(item);
    //                }            
    //                break;
    //        }
    //    }

    //    public void Move(ItemId parentId, ItemId newParentId, Item item)
    //    {
    //        this.Move(parentId, newParentId, item, this.itemRepository.GetLastChildId(newParentId));
    //    }

    //    public void Move(ItemId parentId, ItemId newParentId, Item item, ItemId predecessorId)
    //    {
    //        if (ItemId.IsEmpty(parentId))
    //            throw new ArgumentException(ExceptionMessages.IdEmpty, nameof(parentId));
    //        if (ItemId.IsEmpty(newParentId))
    //            throw new ArgumentException(ExceptionMessages.IdEmpty, nameof(newParentId));
    //        if (item == null)
    //            throw new ArgumentNullException(nameof(item));
    //        if (newParentId == predecessorId)
    //            throw new ArgumentException("Specified Parent ID cannot be equal to Predecessor ID.");
            
    //        Item successor = null;
    //        item.RemoveParent(parentId);
    //        item.AddParent(newParentId);
    //        if (!ItemId.IsEmpty(predecessorId))
    //        {
    //            var successorId = this.itemRepository.GetNextSiblingId(newParentId, predecessorId); 
    //            item.UpdatePredecessor(newParentId, predecessorId);
    //            if(!ItemId.IsEmpty(successorId))
    //            {
    //                successor = this.itemRepository.GetById(successorId);
    //                successor.UpdatePredecessor(newParentId, item.Id);
    //            }                
    //        }

    //        this.itemRepository.Save(item);
    //        if (successor != null)
    //            this.itemRepository.Save(successor);
    //    }
    //}
}
