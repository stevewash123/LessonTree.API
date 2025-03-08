using AutoMapper;
using LessonTree.DAL.Domain;
using LessonTree.DAL.Repositories;
using LessonTree.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LessonTree.BLL.Service
{
    public class StandardService : IStandardService
    {
        private readonly IStandardRepository _repository;
        private readonly IMapper _mapper;

        public StandardService(IStandardRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public List<StandardResource> GetAll()
        {
            var standards = _repository.GetAll().ToList();
            return _mapper.Map<List<StandardResource>>(standards);
        }

        public StandardResource GetById(int id)
        {
            var standard = _repository.GetById(id);
            return standard != null ? _mapper.Map<StandardResource>(standard) : null;
        }

        public void Add(StandardCreateResource standardCreateResource)
        {
            var standard = _mapper.Map<Standard>(standardCreateResource);
            _repository.Add(standard);
        }

        public void Update(StandardUpdateResource standardUpdateResource)
        {
            var standard = _mapper.Map<Standard>(standardUpdateResource);
            _repository.Update(standard);
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public List<StandardResource> GetByTopicId(int topicId)
        {
            var standards = _repository.GetByTopicId(topicId).ToList();
            return _mapper.Map<List<StandardResource>>(standards);
        }
    }
}
