using System;
using System.Threading.Tasks;

namespace SimpleCQRS
{
    public class InventoryCommandHandlers
    {
        private readonly IEventSourcedRepository<InventoryItem> _repository;

        public InventoryCommandHandlers(IEventSourcedRepository<InventoryItem> repository)
        {
            _repository = repository;
        }

        public void Handle(CreateInventoryItem message)
        {
            var item = new InventoryItem(message.InventoryItemId, message.Name);
            _repository.Save(item, -1);
        }

        public async Task Handle(DeactivateInventoryItem message)
        {
            var item = await _repository.GetById(message.InventoryItemId);
            item.Deactivate();
            await _repository.Save(item, message.OriginalVersion);
        }

        public async Task Handle(RemoveItemsFromInventory message)
        {
            var item = await _repository.GetById(message.InventoryItemId);
            item.Remove(message.Count);
            await _repository.Save(item, message.OriginalVersion);
        }

        public async Task Handle(CheckInItemsToInventory message)
        {
            var item = await  _repository.GetById(message.InventoryItemId);
            item.CheckIn(message.Count);
            await _repository.Save(item, message.OriginalVersion);
        }

        public async Task Handle(RenameInventoryItem message)
        {
            var item = await _repository.GetById(message.InventoryItemId);
            item.ChangeName(message.NewName);
            await _repository.Save(item, message.OriginalVersion);
        }
    }
}
