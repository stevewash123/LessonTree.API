using LessonTree.Models.DTO;

public interface IStandardService
{
    List<StandardResource> GetAll();
    StandardResource GetById(int id);
    void Add(StandardCreateResource standardCreateResource);
    void Update(StandardUpdateResource standardUpdateResource);
    void Delete(int id);
    List<StandardResource> GetByTopicId(int topicId);
}