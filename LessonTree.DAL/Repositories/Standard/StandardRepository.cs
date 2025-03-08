using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.DAL;

public class StandardRepository : IStandardRepository
{
    private readonly LessonTreeContext _context;

    public StandardRepository(LessonTreeContext context)
    {
        _context = context;
    }

    public IQueryable<Standard> GetAll()
    {
        return _context.Standards.AsQueryable();
    }

    public Standard GetById(int id)
    {
        return _context.Standards.Find(id);
    }

    public void Add(Standard standard)
    {
        _context.Standards.Add(standard);
        _context.SaveChanges();
    }

    public void Update(Standard standard)
    {
        _context.Standards.Update(standard);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var standard = _context.Standards.Find(id);
        if (standard != null)
        {
            _context.Standards.Remove(standard);
            _context.SaveChanges();
        }
    }

    public IQueryable<Standard> GetByTopicId(int topicId)
    {
        return _context.Standards.Where(s => s.TopicId == topicId);
    }
}