import { createSelector } from 'reselect';

function createQueueItemSelector() {
  return createSelector(
    (state, { albumId }) => albumId,
    (state) => state.queue.details.items,
    (albumId, details) => {
      if (!albumId) {
        return null;
      }

      return details.find((item) => {
        return item.album.id === albumId;
      });
    }
  );
}

export default createQueueItemSelector;
